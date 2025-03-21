<?php

interface ILogger
{
	public function log($logLevel, $message, $label);

	public function crit($message, $exitCode);
	public function error($message);
	public function warn($message);
	public function alert($message);
	public function info($message);
	public function debug($message);

	public function handleError($errno, $errstr, $errfile, $errline);
}

class Logger implements ILogger
{
	private $logToFile = LOG_FILE;
	private $fileLogLevel = LOG_FILE_LEVEL;
	private $logToTerminal = LOG_STDOUT;
	private $terminalLogLevel = LOG_STDOUT_LEVEL;

	private $fileHandle;

	private $logLevels = array(
		'CRIT',
		'ERROR',
		'WARN',
		'ALERT',
		'INFO',
		'DEBUG'
	);

	public function __construct(
		private $botName = 'cryptotrader',
	)
	{
		if ($this->logToFile) {
			$time = new DateTime();
			$this->fileName = "{$this->logToFile}$botName-{$time->format('Y-m-d')}";
			if (file_exists($this->fileName.'.log')) {
				$i = 2;
				while (file_exists($this->fileName."_$i.log")) $i++;
				$this->fileName .= "_$i";
			}
			$this->fileName .= '.log';
			$this->fileHandle = fopen($this->fileName, 'c');
		}
	}

	private function getTimestamp()
	{
		$time = new DateTime();
		return $time->format('Y-m-d H:i:s');
	}

	private function logMessage($level, $message)
	{
		if ($this->logToTerminal && $this->terminalLogLevel >= $level) echo $message;
		if ($this->logToFile && $this->fileHandle !== false && $this->fileLogLevel >= $level) fwrite($this->fileHandle, $message);
	}

	public function log($level, $message, $label = '')
	{
		$ts = $this->getTimestamp();
		$lines = explode("\n", $message);
		foreach($lines as $i=>$line) {
			if ($i === 0) $header = $this->logLevels[$level]."\t$ts |".rpad($label, 3).'|>';
			else if ($i === 1) $header = "\t".lpad('|'.rpad($label, 3).'|>', strlen($ts)+7);
			$this->logMessage($level, "$header\t$line\n");
		}
	}

	public function crit($message, $exitCode = 1) { $this->log(0, $message); exit($exitCode); }
	public function error($message) { $this->log(1, $message); }
	public function warn($message) { $this->log(2, $message); }
	public function alert($message) { $this->log(3, $message); }
	public function info($message) { $this->log(4, $message); }
	public function debug($message) { $this->log(5, $message); }

	public function handleError($errno, $errstr, $errfile, $errline)
	{
		if (!(error_reporting() & $errno)) return false;

		$errLevel = match($errno) {
			E_USER_ERROR => 0,
			E_RECOVERABLE_ERROR => 1,
			E_WARNING,E_USER_WARNING => 2,
			E_NOTICE,E_USER_NOTICE,E_DEPRECATED,E_USER_DEPRECATED => 3,
			default => 5,
		};

		$this->log($errLevel, "$errstr\nIn $errfile, Line $errline", 'PHP');
		return true;
	}

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
		$this->parent->log($level, $message, strlen($label) ? $label : $this->label);
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

