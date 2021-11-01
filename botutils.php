<?

function getArgs($lookingfor)
{
    global $argv;
    foreach($argv as $key=>$argument)
    {
        if(!substr($argument,0,1)=='-') continue;
        $arg = trim(substr($argument,1));
        if(in_array($arg,$lookingfor))
        {
            $args[$arg] = ((substr($argv[($key+1)],0,1)=='-'?true:$argv[($key+1)]));
            if($args[$arg]===NULL)
                $args[$arg] = true;
        }
    }

    return $args;
}

function getTimestamp()
{
	$time = new DateTime();
	return '['.$time->format('Y-m-d H:i:s').']';
}