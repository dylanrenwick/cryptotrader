<?php

abstract class Bot
{
	protected $crypto;
	protected $currency;

	protected $coinsHeld;

	public function __construct(
		protected CoinbaseExchange $cb,
		protected ILogger $log,
		protected bool $sim,
		string $p
	)
    {
		$product = explode('-', $p);
		$this->crypto = $product[0];
		$this->currency = $product[1];
		$this->coinsHeld = 0;
    }

	public abstract function parseProfile(array $config): string|bool;
	public abstract function update(int $step);

	protected function adjustCoinsHeld($amount)
	{
		$this->coinsHeld = max($this->coinsHeld + $amount, 0);
	}

	protected function sellCrypto(float $amount)
	{
		$this->log->alert("Selling $amount ".$this->crypto);
		if ($this->sim) return;
		$coinsBefore = $this->getCryptoBalance();
		$this->cb->marketSellCrypto($amount, $this->crypto.'-'.$this->currency);
		$coinsAfter = $this->getCryptoBalance();
		$coinsSold = $coinsBefore - $coinsAfter;
		$this->adjustCoinsHeld($coinsSold * -1);
	}

	protected function buyCrypto(float $amount)
	{
		$this->log->alert("Buying $amount ".$this->crypto);
		if ($this->sim) return;
		$coinsBefore = $this->getCryptoBalance();
		$this->cb->marketBuyCrypto($amount, $this->crypto.'-'.$this->currency);
		$coinsAfter = $this->getCryptoBalance();
		$coinsBought = $coinsAfter - $coinsBefore;
		$this->adjustCoinsHeld($coinsBought);
	}

	protected function getCryptoAccountInfo()
	{
		return $this->cb->getAccountInfo($this->crypto);
	}
	protected function getCryptoBalance()
	{
		$account = $this->getCryptoAccountInfo();
		$balance = 0.0;
		if (isset($account['balance']))	$balance = $account['balance'];
		return $balance;
	}
}
