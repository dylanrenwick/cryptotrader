<?php

class Waverider extends Bot
{
	private $budget;
	private $gain;
	private $plumetValue;
	private $buyOnStart;
	private $priceOnStart;

	private $priceSoldAt;
	private $coinsHeld;

	private $lastUpdate;
	private $startTime;

	private $highestPrice = 0;
	private $lowestPrice = PHP_INT_MAX;
	private $transactionCount = 0;

	public function parseArgs($args)
	{
		$args = parseArgs($args, array(
			'bw' => array('required' => true),
			'g' => array('required' => true),
			'pv' => array('required' => true),
			'nib' => array('default' => false),
			'fip' => array(),
		));

		$this->budget = floatval($args['bw']);
		$this->gain = floatval($args['g']) / 100;
		$this->plumetValue = floatval($args['pv']) / 100;

		$this->buyOnStart = !$args['nib'];
		if (!$this->buyOnStart) $this->priceOnStart = floatval($args['fip']);

		$argsLog = 'Args:'."\n".
			'budget: $'.$args['bw']."\n".
			'gain: '.($this->gain * 100)."%\n".
			'plumet val: '.($this->plumetValue * 100)."%\n".
			'buy on start: '.($this->buyOnStart ? 'true': 'false');
		if (!$this->buyOnStart) $argsLog .= "\n".'initial price: $'.$this->priceOnStart;
		$this->log->debug($argsLog);

		$this->sellTarget = round($this->budget * $this->gain + $this->budget, 4);
		$this->log->debug('Intial sell target price is '.$this->sellTarget);
	}

	public function startup()
	{
		$this->log->alert('WaveRider starting...');
		$this->log->info('Trading '.$this->crypto.'-'.$this->currency);
		$priceBoughtAt = $this->buyOnStart ? $this->cb->lastaskprice : $this->priceOnStart;
		$this->log->info('Initial price is $'.$priceBoughtAt);
		$this->priceSoldAt = $this->cb->lastbidprice;

		$this->coinsHeld = round($this->budget / $priceBoughtAt, 7);
		$this->log->info('Starting with '.$this->coinsHeld.' '.$this->crypto);

		if ($this->buyOnStart) {
			$this->buyCrypto($this->coinsHeld);
		} else {
			$this->log->info('Buy on start is disabled, assuming coins already bought');
		}

		$this->lastUpdate = time();
		$this->startTime = new DateTime();
	}

	public function update()
	{
		if ($this->coinsHeld !== false) {
			$this->log->info('Currently have '.$this->coinsHeld.' '.$this->crypto.'. Looking to sell.');
			$this->handleSell();
		} else {
			$this->log->info('Currencly have no coins. Looking to buy.');
			$this->handleBuy();
		}

		if (time() - $this->lastUpdate >= BOT_ALERT_INTERVAL) {
			$this->alertUpdate();
		}
	}

	private function alertUpdate()
	{
		$this->lastUpdate = time();

		$alertMsg = 'Waverider has been running since '.$this->startTime->format('Y-m-d H:i:s')."\n".
			'Stats for last '.(BOT_ALERT_INTERVAL/(60*60)).' hours:'."\n".
			'Highest price seen: $'.$this->highestPrice."\n".
			'Lowest price seen: $'.$this->lowestPrice."\n".
			'Transactions made: '.$this->transactionCount."\n";
		$this->log->alert($alertMsg);

		$this->highestPrice = 0;
		$this->lowestPrice = PHP_INT_MAX;
		$this->transactionCount = 0;
	}

	private function handleSell()
	{
		$this->priceSoldAt = $this->cb->lastbidprice;
		$this->highestPrice = max($this->highestPrice, $this->priceSoldAt);
		$this->lowestPrice = min($this->lowestPrice, $this->priceSoldAt);
		$this->log->debug('Current sell price: $'.$this->priceSoldAt);
		$currentSellValue = $this->priceSoldAt * $this->coinsHeld;
		$profit = $currentSellValue - $this->budget;
		$this->log->info('Can sell '.$this->coinsHeld.' '.$this->crypto." for $$currentSellValue, profiting $$profit");
		if ($profit >= $this->budget * $this->gain) {
			$this->log->debug("Profit $$profit is greater than target of $".($this->budget * $this->gain).'. Attempting to sell.');
			$this->sellCrypto($this->coinsHeld);
			$this->transactionCount++;
			$this->coinsHeld = false;
		} else {
			$this->log->info('Waiting until profit of $'.($this->budget * $this->gain));
		}
	}
	private function handleBuy()
	{
		$buyPrice = $this->cb->lastaskprice;
		$this->highestPrice = max($this->highestPrice, $buyPrice);
		$this->lowestPrice = min($this->lowestPrice, $buyPrice);
		$targetPrice = round($this->priceSoldAt * (1 - $this->plumetValue), 4);
		$this->log->info("Can buy at $$buyPrice.");
		if ($buyPrice <= $targetPrice) {
			$this->coinsHeld = round($this->budget / $buyPrice, 7);
			$this->log->debug("Price is $$buyPrice, below target of $$targetPrice. Attempting to buy ".$this->coinsHeld.' '.$this->crypto);
			$this->buyCrypto($this->coinsHeld);
			$this->transactionCount++;
		} else {
			$this->log->info("Waiting for $$targetPrice");
		}
	}
}
