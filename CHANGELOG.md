# v0.3.6
* Fixed calculating how many coins to sell instead of selling based on `coinsHeld`

# v0.3.5
* Added `initial_last_sold_price` config option to set last sold price when starting in buying state
* Updated formatting of bot config output to display all config keys and identify unknown keys
* Fixed bot state not changing to `state_wait_sell` after buying crypto
* Fixed transactions counter in periodic alert not being incremented
* Fixed last sold price not printing when transitioning from `state_wait_sell` to `state_sell`

# v0.3.4
* Fixed wrong variable name used in buying and selling
* Fixed 'size is too accurate' error when selling by rounding to 8 decimal places

# v0.3.3
* Fixed faulty percentage calculation causing bot to wait too long before selling
* Fixed double percent signs in log

## v0.3.2
* Fixed faulty error checking on fetching coins held causing values to round to 1
* Added additional debug logging when loading coins from API

## v0.3.1
* Added debug logging to loading accounts from API

# v0.3.0
* Started Changelog
* Added functionality to track coins held via API
