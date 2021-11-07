<?php

class Logger
{
	private $logToFile = LOG_FILE;
	private $logToTerminal = LOG_STDOUT;
	private $logLevel = LOG_LEVEL;

	private $logLevels = array(
		'CRIT',
		'ERROR',
		'WARN',
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

	public function log($level, $message)
	{
		$ts = $this->getTimestamp();
		$lines = explode("\n", $message);
		foreach($lines as $line) {
			$this->logMessage($this->logLevels[$level]."\t$ts |>\t$line\n");
			$ts = lpad('', strlen($ts));
		}
	}

	public function crit($message) { $this->log(0, $message); }
	public function error($message) { $this->log(1, $message); }
	public function warn($message) { $this->log(2, $message); }
	public function info($message) { $this->log(3, $message); }
	public function debug($message) { $this->log(4, $message); }
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

