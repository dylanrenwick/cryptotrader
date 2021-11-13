<?php

define('VERSION', 'v0.1.0');

require_once './coinbase-pro.php';
require_once './config.inc.php';
require_once './logger.php';

abstract class Bot
{
	protected $crypto;
	protected $currency;

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
    }

	public abstract function parseConfig(array $config): string|bool;
	public abstract function update();

	protected function sellCrypto(float $amount)
	{
		$this->log->alert("Selling $amount ".$this->crypto);
		if ($this->sim) return;
		$this->cb->marketSellCrypto($amount, $this->crypto.'-'.$this->currency);
	}

	protected function buyCrypto(float $amount)
	{
		$this->log->alert("Buying $amount ".$this->crypto);
		if ($this->sim) return;
		$this->cb->marketBuyCrypto($amount, $this->crypto.'-'.$this->currency);
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
$L->debug("Args:\nbotfile: {$args['bot']}\nproduct: {$args['p']}\nconfig: {$args['cfg']}");
if ($args['sim']) {
	$L->alert('-= [Simulation] =-');
	$L->debug('In simulation mode, the bot will not send transactions to Coinbase.');
}
$L->info('Loading bot config from '.$args['cfg']);

if (!file_exists($args['cfg'])) {
	$L->crit('Could not load bot config: File not found');
}
require $args['cfg'];
if ($BOT_CONFIG === null) $L->crit('Count not load bot config: BOT_CONFIG not defined');
if (!is_array($BOT_CONFIG)) $L->crit('Could not load bot config: Expected BOT_CONFIG to be array');

$L->info('Initializing API');
$cb = new CoinbaseExchange(CB_KEY, CB_SECRET, CB_PASSPHRASE, $L->createLabelledLogger('API'));
$cb->updatePrices($args['p']);
$L->alert('API initialized.');

$L->info('Initializing bot: '.$args['bot']);
require BOT_DIRECTORY.$args['bot'].'.php';
if (!is_subclass_of($args['bot'], 'Bot')) $L->crit($args['bot'].'.php does not contain a valid bot class!'."\n\t".'Does not extend Bot');

$bot = new $args['bot']($cb, $L->createLabelledLogger('BOT'), $args['sim'], $args['p']);
$L->debug('Parsing bot config');
$bot->parseConfig($BOT_CONFIG);
$L->debug('Running bot startup');
$bot->startup();
$L->alert('Bot initialized.');

$L->alert('Running');
while(1) {
	$cb->updatePrices($args['p']);

	$L->debug('Checking for config updates...');
	$temp = $BOT_CONFIG;
	require $args['cfg'];
	if ($temp != $BOT_CONFIG) {
		$L->alert('Detected config change, updating bot config');
		$bot->parseConfig($BOT_CONFIG);
	}

	$bot->update();

	sleep(BOT_INTERVAL);
}

