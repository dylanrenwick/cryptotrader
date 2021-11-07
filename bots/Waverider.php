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

	function parseArgs($args)
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

		$this->sellTarget = round($this->budget * $this->gain + $this->budget, 4);
	}

	function startup()
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
	}

	function update()
	{
		if ($this->coinsHeld !== false) {
			$this->log->info('Currently have '.$this->coinsHeld.' '.$this->crypto.'. Looking to sell.');
			$this->handleSell();
		} else {
			$this->log->info('Currencly have no coins. Looking to buy.');
			$this->handleBuy();
		}
	}

	function handleSell()
	{
		$this->priceSoldAt = $this->cb->lastbidprice;
		$currentSellValue = $this->priceSoldAt * $this->coinsHeld;
		$profit = $currentSellValue - $this->budget;
		$this->log->info('Can sell '.$this->coinsHeld.' '.$this->crypto." for $$currentSellValue, profiting $$profit");
		if ($profit >= $this->budget * $this->gain) {
			$this->sellCrypto($this->coinsHeld);
			$this->coinsHeld = false;
		} else {
			$this->log->info('Waiting until profit of $'.($this->budget * $this->gain));
		}
	}
	function handleBuy()
	{
		$buyPrice = $this->cb->lastaskprice;
		$targetPrice = round($this->priceSoldAt * (1 - $this->plumetValue), 4);
		$this->log->info("Can buy at $$buyPrice.");
		if ($buyPrice <= $targetPrice) {
			$this->coinsHeld = round($this->budget / $buyPrice, 7);
			$this->buyCrypto($this->coinsHeld);
		} else {
			$this->log->info("Waiting for $$targetPrice");
		}
	}
}
