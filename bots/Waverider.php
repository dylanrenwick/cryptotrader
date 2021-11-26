<?php

class BotState
{
	public const startup = 0;
	public const selling = 1;
	public const buying = 2;
	public const waitingToSell = 3;
	public const waitingToBuy = 4;

	public static function getStateName(int $state): string {
		return 'state_'.match($state) {
			BotState::startup => 'startup',
			BotState::selling => 'selling',
			BotState::buying => 'buying',
			BotState::waitingToSell => 'sell_wait',
			BotState::waitingToBuy => 'buy_wait',
			default => 'unknown'
		};
	}
}

class Waverider extends Bot
{
	private const KNOWN_PROFILE_KEYS = array(
		// Required
		'buy_amount',
		'min_gain',
		//Optional
		'min_loss',
		'loss_before_sell',
		'no_initial_buy',
		'initial_price',
		'initial_last_sold_price',
		'gain_before_buy',
		'initial_state',
	);
	
	private $buyAmount;
	private $minGain;
	private $minLoss;
	private $buyOnStart;
	private $priceOnStart;
	private $lossBeforeSell;
	private $gainBeforeBuy;

	private $priceBoughtAt;
	private $priceSoldAt;

	private $lastUpdate;
	private $startTime;

	private $highestPrice = 0;
	private $lowestPrice = PHP_INT_MAX;
	private $transactionCount = 0;

	private $botState;
	private $sellPeak;
	private $buyPeak;

	public function parseProfile(array $profile): string|bool
	{
		// Required
		if (!isset($profile['buy_amount'])) return '"buy_amount" not specified';
		if (!isset($profile['min_gain'])) return '"min_gain" not specified';
		
		// Optional
		if (!isset($profile['min_loss'])) $profile['min_loss'] = 0;
		if (!isset($profile['loss_before_sell'])) $profile['loss_before_sell'] = 0;
		if (!isset($profile['no_initial_buy'])) $profile['no_initial_buy'] = false;
		if (!isset($profile['initial_price'])) {
			if (!isset($profile['no_initial_buy']) || !$profile['no_initial_buy']) $profile['initial_price'] = 0;
			else return '"no_initial_buy" is enabled but "initial_price" not specified';
		}
		if (!isset($profile['initial_last_sold_price'])) $profile['initial_last_sold_price'] = False;
		if (!isset($profile['gain_before_buy'])) $profile['gain_before_buy'] = 0;
		if (!isset($profile['initial_state'])) $profile['initial_state'] = BotState::startup;

		$this->buyAmount = floatval($profile['buy_amount']);
		$this->minGain = floatval($profile['min_gain']) / 100;
		$this->minLoss = floatval($profile['min_loss']) / 100;

		$this->lossBeforeSell = $profile['loss_before_sell'];
		$this->gainBeforeBuy = $profile['gain_before_buy'];

		$this->buyOnStart = !$profile['no_initial_buy'];
		if (!$this->buyOnStart) {
			$this->priceOnStart = floatval($profile['initial_price']);
			$this->priceSoldAt = floatval($profile['initial_last_sold_price']);
		}

		$argsLog = "Parsed Profile:\n";
		uksort($profile, function($a, $b) {
			$a_val = intval(in_array($a, Waverider::KNOWN_PROFILE_KEYS));
			$b_val = intval(in_array($b, Waverider::KNOWN_PROFILE_KEYS));
			return 1 - ($a_val - $b_val);
		});
		foreach ($profile as $key => $value) {
			$argsLog .= "- $key: ".var_export($value, true);
			if (!in_array($key, Waverider::KNOWN_PROFILE_KEYS)) {
				$argsLog .= "\t; unrecognized profile key";
			}
			$argsLog .= "\n";
		}
		if (!$this->buyOnStart) $argsLog .= "\ninitial price: \${$this->priceOnStart}";
		$this->log->debug($argsLog);

		$this->sellTarget = round($this->buyAmount * $this->minGain + $this->buyAmount, 4);
		$this->log->debug("Intial sell target price is {$this->sellTarget}");

		if (!$this->buyOnStart) {
			if (!isset($profile['initial_balance'])) {
				$this->log->debug('initial_balance not profileured, fetching balance from API.');
				$this->cb->loadAccounts();
				$profile['initial_balance'] = $this->getCryptoBalance();
				$this->log->debug('Fetched '.$this->getCryptoBalance());
			}
			$this->coinsHeld = $profile['initial_balance'];
			$this->log->debug('Starting with '.$this->coinsHeld.' coins');
		}

		$this->setBotState($profile['initial_state']);

		return true;
	}

	public function startup()
	{
		$this->log->alert('WaveRider starting...');
		$this->log->info("Trading {$this->crypto}-{$this->currency}");
		$this->priceBoughtAt = $this->buyOnStart ? $this->cb->lastaskprice : $this->priceOnStart;
		$this->log->info("Initial price is \${$this->priceBoughtAt}");
		if (empty($this->priceSoldAt)) $this->priceSoldAt = $this->cb->lastbidprice;

		$this->lastUpdate = time();
		$this->startTime = new DateTime();
	}

	public function update(int $step)
	{
		$this->log->debug("Update step $step at interval of ".BOT_INTERVAL.'s');
		$this->log->debug('Current bot state is '.BotState::getStateName($this->botState));
		match ($this->botState) {
			BotState::startup => $this->handleStartup(),
			BotState::selling => $this->handleSell(),
			BotState::buying => $this->handleBuy(),
			BotState::waitingToSell => $this->handleSellWait(),
			BotState::waitingToBuy => $this->handleBuyWait(),
		};

		if (time() - $this->lastUpdate >= BOT_ALERT_INTERVAL) {
			$this->alertUpdate();
		}
	}

	private function alertUpdate()
	{
		$this->lastUpdate = time();

		$alertMsg = "Waverider has been running since {$this->startTime->format('Y-m-d H:i:s')}\n".
			'Stats for last '.(BOT_ALERT_INTERVAL/(60*60))." hours:\n".
			"Highest price seen: \${$this->highestPrice}\n".
			"Lowest price seen: \${$this->lowestPrice}\n".
			"Transactions made: {$this->transactionCount}\n";
		$this->log->alert($alertMsg);

		$this->highestPrice = 0;
		$this->lowestPrice = PHP_INT_MAX;
		$this->transactionCount = 0;
	}

	private function setBotState(int $botState)
	{
		$this->botState = $botState;
		$this->inStateSince = new DateTime();
	}

	private function handleStartup()
	{
		$this->coinsToBuy = round($this->buyAmount / $this->priceBoughtAt, 7);
		$this->log->info("Starting with {$this->coinsToBuy} {$this->crypto}");

		if ($this->buyOnStart) {
			$this->buyCrypto($this->coinsToBuy);
		} else {
			$this->log->info('Buy on start is disabled, assuming coins already bought');
		}

		$this->setBotState(BotState::waitingToSell);
	}

	private function getSellProfit()
	{
		$sellPrice = $this->cb->lastbidprice;
		$this->highestPrice = max($this->highestPrice, $sellPrice);
		$this->lowestPrice = min($this->lowestPrice, $sellPrice);
		$this->log->debug("Current sell price: $$sellPrice");
		$profit = $sellPrice - $this->priceBoughtAt;
		$this->log->info("Can sell {$this->coinsHeld} {$this->crypto} for $".($sellPrice * $this->coinsHeld).', profiting $'.($profit * $this->coinsHeld));
		return $profit;
	}
	private function handleSellWait()
	{
		$profit = $this->getSellProfit();
		if ($profit >= $this->priceBoughtAt * $this->minGain) {
			$this->log->debug("Profit $$profit is greater than target of $".($this->buyAmount * $this->minGain).'. Attempting to sell.');
			$this->sellPeak = $this->cb->lastbidprice;
			$this->setBotState(BotState::selling);
		} else {
			$this->log->info('Waiting until profit of $'.($this->buyAmount * $this->minGain));
		}
	}
	private function handleSell()
	{
		$this->sellPeak = max($this->sellPeak, $this->cb->lastbidprice);
		$this->log->debug("Peak sell price is \${$this->sellPeak}. Current sell price is \${$this->cb->lastbidprice}");
		if ($this->cb->lastbidprice < $this->sellPeak) {
			$loss = ($this->sellPeak - $this->cb->lastbidprice) / $this->sellPeak * 100;
			$this->log->debug("Price dropped by $loss% since reaching high of \${$this->sellPeak}");
			if ($loss >= $this->lossBeforeSell) {
				$this->log->alert("Loss of $loss% is greater than threshold of {$this->lossBeforeSell}%. Price is dropping, selling coins.");
				$coinsToSell = round($this->buyAmount / $this->priceBoughtAt, 8);
				$this->sellCrypto($coinsToSell);
				$this->transactionCount++;
				$this->setBotState(BotState::waitingToBuy);
			}
		}
	}
	
	private function getBuyProfit()
	{
		$buyPrice = $this->cb->lastaskprice;
		$this->highestPrice = max($this->highestPrice, $buyPrice);
		$this->lowestPrice = min($this->lowestPrice, $buyPrice);
		$this->log->debug("Current buy price: $$buyPrice");
		$profit = $this->priceSoldAt - $buyPrice;
		$this->log->info('Can buy '.($this->buyAmount / $buyPrice)." {$this->crypto} for \${$this->buyAmount}, which is ".($profit / $this->priceSoldAt).'% cheaper than last sell.');
		return $profit;
	}
	private function handleBuyWait()
	{
		$profit = $this->getBuyProfit();
		if ($profit > $this->priceSoldAt * $this->minLoss) {
			$this->log->debug("Price is \${$this->cb->lastaskprice}, at least {$this->minLoss}% below last sell price of \${$this->priceSoldAt}. Attempting to buy.");
			$this->buyPeak = $this->cb->lastaskprice;
			$this->setBotState(BotState::buying);
		} else {
			$this->log->info("Waiting for price drop of at least {$this->minLoss}% since last sell.");
		}
	}
	private function handleBuy()
	{
		$this->buyPeak = min($this->buyPeak, $this->cb->lastaskprice);
		$this->log->debug("Peak buy price is \${$this->buyPeak}. Current buy price is \${$this->cb->lastaskprice}");
		if ($this->cb->lastaskprice > $this->buyPeak) {
			$gain = ($this->cb->lastaskprice - $this->buyPeak) / $this->buyPeak * 100;
			$this->log->debug("Price raised by $gain% since reaching low of \${$this->buyPeak}");
			if ($gain >= $this->gainBeforeBuy) {
				$this->log->alert("Rise of $gain% is greater than threshold of {$this->gainBeforeBuy}%. Price is rising, buying coins.");
				$this->buyCrypto($this->buyAmount);
				$this->transactionCount++;
				$this->setBotState(BotState::waiitingToSell);
			}
		}
	}
}
