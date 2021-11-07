<?php

class TestBot extends Bot
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
		$this->gain = floatval($args['g']);
		$this->plumetValue = floatval($args['pv']);

		$this->buyOnStart = !$args['nib'];
		if (!$this->buyOnStart) $priceOnStart = floatval($args['fip']);

		$this->sellTarget = round($this->budget * ($this->gain / 100) + $this->budget, 4);
	}

	function startup()
	{
		$priceBoughtAt = $this->buyOnStart ? $this->cb->lastaskprice : $this->priceOnStart;
		$this->priceSoldAt = $this->cb->lastbidprice;

		$this->coinsHeld = round($this->budget / $priceBoughtAt, 7);

		if ($this->buyOnStart) {
			$this->buyCrypto($this->coinsHeld);
		}
	}

	function update()
	{
		if ($this->coinsHeld !== false) {
			$this->handleSell();
		} else {
			$this->handleBuy();
		}

		echo "Running...\n";
	}

	function handleSell()
	{
		$this->priceSoldAt = $this->cb->lastbidprice;
		$currentSellValue = $this->priceSoldAt * $this->coinsHeld;
		$profit = $currentSellValue - $this->budget;
		if ($profit >= $this->budget * $this->gain) {
			$this->sellCrypto($this->coinsHeld);
			$this->coinsHeld = false;
		}
	}
	function handleBuy()
	{
		$buyPrice = $this->cb->lastaskprice;
		$targetPrice = round($this->priceSoldAt * (1 - $this->plumetValue), 4);
		if ($buyPrice <= $targetPrice) {
			$this->coinsHeld = round($this->budget / $buyPrice, 7);
			$this->buyCrypto($this->coinsHeld);
		}
	}
}
