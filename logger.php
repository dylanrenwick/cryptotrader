<?php

interface ILogger
{
	public function log($logLevel, $message, $label);

	public function crit($message);
	public function error($message);
	public function warn($message);
	public function alert($message);
	public function info($message);
	public function debug($message);
}

class Logger implements ILogger
{
	private $logToFile = LOG_FILE;
	private $logToTerminal = LOG_STDOUT;
	private $logLevel = LOG_LEVEL;

	private $logLevels = array(
		'CRIT',
		'ERROR',
		'WARN',
		'ALERT',
		'INFO',
		'DEBUG'
	);

	private function getTimestamp()
	{
		$time = new DateTime();
		return $time->format('Y-m-d H:i:s');
	}

	private function logMessage($message)
	{
		if ($this->logToTerminal) echo $message;
	}

	public function log($level, $message, $label = '')
	{
		if ($level > $this->logLevel) return;
		$ts = $this->getTimestamp();
		$lines = explode("\n", $message);
		foreach($lines as $i=>$line) {
			if ($i === 0) $header = $this->logLevels[$level]."\t$ts |".rpad($label, 3).'|>';
			else if ($i === 1) $header = "\t".lpad('|'.rpad($label, 3).'|>', strlen($ts)+7);
			$this->logMessage("$header\t$line\n");
		}
	}

	public function crit($message) { $this->log(0, $message); }
	public function error($message) { $this->log(1, $message); }
	public function warn($message) { $this->log(2, $message); }
	public function alert($message) { $this->log(3, $message); }
	public function info($message) { $this->log(4, $message); }
	public function debug($message) { $this->log(5, $message); }

	public function createLabelledLogger($label)
	{
		return new Sublogger($this, $label);
	}
}

class SubLogger extends Logger
{
	private $parent;
	private $label;

	public function __construct($parent, $label)
	{
		$this->parent = $parent;
		$this->label = $label;
	}

	public function log($level, $message, $label = '')
	{
		$this->parent->log($level, $message, $this->label);
	}
}

function lpad($str, $len, $ch = ' ')
{
	if (strlen($str) > $len) return substr($str, 0, $len);
	else return str_repeat($ch, $len - strlen($str)) . $str;
}
function rpad($str, $len, $ch = ' ')
{
	if (strlen($str) > $len) return substr($str, 0, $len);
	else return $str . str_repeat($ch, $len - strlen($str));
}

