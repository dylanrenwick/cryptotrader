<?php 
date_default_timezone_set('UTC');

// enter key, secret and passphrase from Coinbase Pro
// get them from:
//          https://pro.coinbase.com/profile/api
define('CB_KEY','your key here');
define('CB_SECRET','your secret here');
define('CB_PASSPHRASE','your passphrase here');

define('BOT_DIRECTORY', './bots/');
define('BOT_INTERVAL', 5);
define('BOT_ALERT_INTERVAL', (60*60)*12);

define('LOG_FILE', false);
define('LOG_FILE_LEVEL', 5);
define('LOG_STDOUT', true);
define('LOG_STDOUT_LEVEL', 3);

