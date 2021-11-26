<?php

define('VERSION', 'v0.3.5');

require_once './coinbase-pro.php';
require_once './config.inc.php';
require_once './logger.php';

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

function getArgs(): array
{
    global $argv;
    $args = array();
    foreach($argv as $key=>$argument)
    {
        if (substr($argument,0,1)!='-') continue;
		$arg = trim(substr($argument,1));
		if ($key < count($argv) - 1) {
			$next = $argv[$key+1];
			$args[$arg] = (substr($next,0,1)=='-' ? true : $next);
		} else {
			$args[$arg] = true;
		}
    }
    return $args;
}

function parseArgs(array $args, array $argOpts): array
{
	$args = array_filter($args, function($key) use ($argOpts) {
		return array_key_exists($key, $argOpts);
	}, ARRAY_FILTER_USE_KEY);

    foreach($argOpts as $argKey=>$argData)
	{
		$required = isset($argData['required']) && $argData['required'];
		if (!isset($args[$argKey])) {
			if ($required) exit('Argument required: '.$argKey);
			else if (isset($argData['default'])) $args[$argKey] = $argData['default'];
		}
	}

	return $args;
}

$args = parseArgs(getArgs(), array(
	'bot' => array('required' => true),
	'p' => array('required' => true),
	'cfg' => array('required' => true),
	'sim' => array('required' => false, 'default' => false),
));

$botName = explode('/', $args['cfg']);
$botName = explode('.', $botName[count($botName) - 1]);
$botName = $botName[0];
$L = new Logger($botName);
$L->debug('Logger initialized.');
set_error_handler(array($L, 'handleError'), E_ALL);
$L->debug('Logger now catching PHP errors.');

$L->alert('-= Cryptotrader '.VERSION.' =-');
$L->alert('--==={ '.$botName.' }===--');
$L->debug("Args:\nbotfile: {$args['bot']}\nproduct: {$args['p']}\nprofile: {$args['cfg']}");
if ($args['sim']) {
	$L->alert('-= [Simulation] =-');
	$L->debug('In simulation mode, the bot will not send transactions to Coinbase.');
}
$L->info('Loading bot profile from '.$args['cfg']);

if (!file_exists($args['cfg'])) {
	$L->crit('Could not load bot profile: File not found');
}
require $args['cfg'];
if ($BOT_PROFILE === null) $L->crit('Count not load bot profile: BOT_PROFILE not defined');
if (!is_array($BOT_PROFILE)) $L->crit('Could not load bot profile: Expected BOT_PROFILE to be array');

$L->info('Initializing API');
$cb = new CoinbaseExchange(CB_KEY, CB_SECRET, CB_PASSPHRASE, $L->createLabelledLogger('API'));
$cb->updatePrices($args['p']);
$L->alert('API initialized.');

$L->info('Initializing bot: '.$args['bot']);
require BOT_DIRECTORY.$args['bot'].'.php';
if (!is_subclass_of($args['bot'], 'Bot')) $L->crit($args['bot'].'.php does not contain a valid bot class!'."\n\t".'Does not extend Bot');

$bot = new $args['bot']($cb, $L->createLabelledLogger('BOT'), $args['sim'], $args['p']);
$L->debug('Parsing bot profile');
$bot->parseProfile($BOT_PROFILE);
$L->debug('Running bot startup');
$bot->startup();
$L->alert('Bot initialized.');

$step = 0;

$L->alert('Running');
while(1) {
	$step++;
	if ($step < 0) $step = 1;
	$cb->updatePrices($args['p']);

	$L->debug('Checking for profile updates...');
	$temp = $BOT_PROFILE;
	require $args['cfg'];
	if ($temp != $BOT_PROFILE) {
		$L->alert('Detected profile change, updating bot profile');
		$bot->parseConfig($BOT_PROFILE);
	}

	$bot->update($step);

	sleep(BOT_INTERVAL);
}

