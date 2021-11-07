<?php

define('VERSION', 'v0.1.0');

require_once './coinbase-pro.php';
require_once './config.inc.php';
require_once './logger.php';

$L = new Logger();

abstract class Bot
{
	protected $cb;
	protected $log;
	protected $sim;

	protected $crypto;
	protected $currency;

    function __construct(CoinbaseExchange $cb, ILogger $log, $p, $sim)
    {
		$this->cb = $cb;
		$this->log = $log;
		$product = explode('-', $p);
		$this->crypto = $product[0];
		$this->currency = $product[1];
		$this->sim = $sim;
    }

	abstract function parseArgs($args);
	abstract function update();

	protected function sellCrypto($amount)
	{
		$this->log->alert("Selling $amount ".$this->crypto);
		if ($this->sim) return;
		$this->cb->marketSellCrypto($amount, $this->crypto.'-'.$this->currency);
	}

	protected function buyCrypto($amount)
	{
		$this->log->alert("Buying $amount ".$this->crypto);
		if ($this->sim) return;
		$this->cb->marketBuyCrypto($amount, $this->crypto.'-'.$this->currency);
	}
}

function getArgs()
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

function parseArgs($args, $argOpts)
{
	$args = array_filter($args, function($key) use ($argOpts) {
		return array_key_exists($key, $argOpts);
	}, ARRAY_FILTER_USE_KEY);

    foreach($argOpts as $argKey=>$argData)
    {
		if (!isset($args[$argKey])) {
			if ($argData['required']) exit('Argument required: '.$argKey);
			else if (isset($argData['default'])) $args[$argKey] = $argData['default'];
		}
	}

	return $args;
}

$args = parseArgs(getArgs(), array(
    'bot' => array('required' => true),
    'p' => array('required' => true),
    'sim' => array('required' => false, 'default' => false)
));

$L->alert('-= Cryptotrader '.VERSION.' =-');
$L->debug('Args:'."\n".'botfile: '.$args['bot']."\n".'product: '.$args['p']);
if ($args['sim']) $L->alert("-== SIMULATION ==-\nIn simulation mode, bots will run without\nactually spending money");

$L->info('Initializing API');
$cb = new CoinbaseExchange(CB_KEY, CB_SECRET, CB_PASSPHRASE, $L->createLabelledLogger('API'));
$cb->updatePrices($args['p']);
$L->alert('API initialized.');

$L->info('Initializing bot: '.$args['bot']);
require BOT_DIRECTORY.$args['bot'].'.php';
if (!is_subclass_of($args['bot'], 'Bot')) {
	exit($args['bot'].'.php does not contain a valid bot class!'."\n\t".'Does not extend Bot');
}

$bot = new $args['bot']($cb, $L->createLabelledLogger('BOT'), $args['p'], $args['sim']);
$L->debug('Parsing bot args');
$bot->parseArgs(getArgs());
$L->debug('Running bot startup');
$bot->startup();
$L->alert('Bot initialized.');

$L->alert('Running');
while(1) {
	$cb->updatePrices($args['p']);

	$bot->update();

	sleep(BOT_INTERVAL);
}
