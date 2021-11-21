<?php
define('DS', DIRECTORY_SEPARATOR);
define('ROOT', dirname(__FILE__));
error_reporting(E_ALL & ~E_NOTICE);

include_once(ROOT.DS.'config.inc.php');

/* Written in accordance to https://docs.pro.coinbase.com/ 
 Author: Christian Haschek <christian@haschek.at>
 Github repo: https://github.com/HaschekSolutions/cryptotrader
 June 2017
*/

class CoinbaseExchange
{
    private $apiurl = "https://api.pro.coinbase.com";
    public $accounts;
    
    private $bidsprices;
    public $lastbidprice = 0;
    private $lowestask = PHP_INT_MAX;
    
    private $askprices;
    public $lastaskprice = 0;
	private $highestask = 0;

	public function __construct(
		private $key,
		private $secret,
		private $passphrase,
		private ILogger $log)
	{ }

    function updatePrices($product='BTC-USD')
	{
		$this->log->info('Updating prices for '.$product);
        $data = $this->makeRequest('/products/'.$product.'/ticker');
        if($data===false){ $this->log->warn("Error getting products");return false;}
        $a = explode('-',$product);
        $crypto=$a[0];
        $currency=$a[1];
        $ask = $data['ask'];
        $bid = $data['bid'];
        $this->askpricese[$product][] = $ask;
        $this->bidprices[$product][] = $bid;

        $out['ask'] = $ask;
        $out['bid'] = $bid;

        if($this->lowestask>$ask)
            $this->lowestask = $ask;
        if($this->highestask<$bid)
            $this->highestask = $bid;

        $this->lastaskprice = $ask;
        $this->lastbidprice = $bid;

        $data = $this->makeRequest('/products/'.$product.'/stats');

        $out['24h_low'] = $data['low'];
        $out['24h_high'] = $data['high'];
        $out['24h_open'] = $data['open'];
        $out['24h_average'] = round(($data['low']+$data['high'])/2,2);

        return $out;
    }

    function printPrices($product='BTC-USD')
    {
        $a = explode('-',$product);
        $crypto=$a[0];
        $currency=$a[1];;

        $this->log->info("Price info for $product\n-----------\n".
			"\tAsk price: \t$this->lastaskprice $currency\n".
			"\tBid price: \t$this->lastbidprice $currency\n".
			"\tSpread: \t".($this->lastaskprice-$this->lastbidprice)." $currency");
    }

    function printAccountInfo()
    {
        echo "[i] Account overview\n-----------------\n";
        foreach($this->accounts as $currency=>$data)
        {
            //if(floatval($data['balance'])<0.0001) continue;
            echo " [i] Currency: $currency\n";
            echo "   [$currency] Total balance: \t\t".$data['balance'].' '.$currency."\n";
            echo "   [$currency] Currently in open orders: \t".$data['hold'].' '.$currency."\n";
            echo "   [$currency] Available: \t\t\t".$data['available'].' '.$currency."\n";
            echo "\n";
        }
    }

    function getAccountInfo($product)
    {
        if(!$this->accounts[$product]) return false;
        return $this->accounts[$product];
    }

    // https://docs.gdax.com/#orders
    function makeOrder($type,$amount,$product='BTC-USD')
    {
        $result = $this->makeRequest('/orders',array(   'size'=>$amount,
                                                        'price'=>1890,
                                                        'side'=>$type,
                                                        'product_id'=>$product
                                                    ));

        return $result;
    }

    function marketSellCrypto($amount,$product='BTC-USD')
	{
		$this->log->info("Making sell request for $amount $product");
		$result = $this->makeRequest(
			'/orders',
			array(
				'size'=>$amount,
				'side'=>'sell',
                'type'=>'market',
                'product_id'=>$product
			)
		);
		$this->loadAccounts();

        return $result;
    }

    function marketSellCurrency($amount,$product='BTC-USD')
    {
		$result = $this->makeRequest(
			'/orders',
			array(
				'funds'=>$amount,
                'side'=>'sell',
                'type'=>'market',
                'product_id'=>$product
			)
		);
		$this->loadAccounts();

        return $result;
    }

    function marketBuyCrypto($amount,$product='BTC-USD')
    {
		$this->log->info("Making buy request for $amount $product");
		$result = $this->makeRequest(
			'/orders',
			array(
				'size'=>$amount,
                'side'=>'buy',
                'type'=>'market',
                'product_id'=>$product
			)
		);
		$this->loadAccounts();

        return $result;
    }

    function marketBuyCurrency($amount,$product='BTC-USD')
    {
		$result = $this->makeRequest(
			'/orders',
			array(
				'funds'=>$amount,
                'side'=>'buy',
                'type'=>'market',
                'product_id'=>$product
			)
		);
		$this->loadAccounts();

        return $result;
    }

    function isOrderDone($orderID)
    {
        $data = $this->makeRequest('/orders/'.$orderID);
        if($data)
        {
            if($data['status']=='done') return true;
            else return false;
        }
        else return true;
    }

    function getOrderInfo($orderID)
    {
        $data = $this->makeRequest('/orders/'.$orderID);
        if($data)
        {
            return $data;
        }
        else return false;
    }

    function loadAccounts()
	{
		$this->log->info('Reloading accounts...');
        $data = $this->makeRequest('/accounts');
        if($data===false) $this->log->error('Error getting accounts');
        foreach($data as $d)
		{
			if ($d['trading_enabled'] === false) {
				$this->log->debug("Skipping account ${d['currency']} because Coinbase reports trading disabled.");
				continue;
			}

			$d['balance'] = floatval($d['balance']);
			$d['hold'] = floatval($d['hold']);
			$d['available'] = floatval($d['available']);

			if ($d['balance'] == 0 && $d['hold'] == 0 && $d['available'] == 0
				&& (
					isset($this->accounts[$d['currency']])
					&& $this->accounts[$d['currency']]['balance'] == 0
					&& $this->accounts[$d['currency']]['hold'] == 0
					&& $this->accounts[$d['currency']]['available'] == 0
				)
			) {
				$this->log->debug("Skipping unused account ${d['currency']}");
				continue;
			}
			$account = var_export($d, true);
			$this->log->debug("Account for ${d['currency']}:\n".$account);

            $this->accounts[$d['currency']] = $d;
        }
    }

    function getHolds($id)
    {
        return $this->makeRequest('/accounts/'.$id.'/holds');
    }

    function makeRequest($path,$postdata='')
    {
        $curl = curl_init();
        if($postdata!='')
        {
            $elements = array();
            foreach($postdata as $key=>$pd)
            {
                $elements[] = $key.'='.$pd;
            }
            $compiledpostdata = implode('&',$elements);
            curl_setopt($curl, CURLOPT_POST, 1);
            curl_setopt($curl, CURLOPT_POSTFIELDS,json_encode($postdata));
        }
        curl_setopt_array($curl, array(
            CURLOPT_RETURNTRANSFER => 1,
            CURLOPT_URL => $this->apiurl.$path,
            CURLOPT_USERAGENT => 'PHPtrader',
            CURLOPT_SSL_VERIFYPEER => false,
            CURLOPT_HTTPHEADER => array('CB-ACCESS-KEY: '.$this->key,
                                        'CB-ACCESS-SIGN: '.$this->signature($path,($postdata==''?'':json_encode($postdata)),time(),(($postdata==''?'GET':'POST'))),
                                        'CB-ACCESS-TIMESTAMP: '.time(),
                                        'CB-ACCESS-PASSPHRASE: '.$this->passphrase,
                                        'Content-Type: application/json'
                                        )
        ));
        $resp = curl_exec($curl);
        if(curl_errno($curl)) return false;

        curl_close($curl);

        if(startsWith($resp,'Cannot')) return false;


        $json = json_decode($resp,true);
        if(isset($json['message']))
        {
            $this->log->error("Error while making a call. Message:\n\t".$json['message']);
            return false;
        }
        else return $json;
    }
    
    public function signature($request_path='', $body='', $timestamp=false, $method='GET') {
        $body = is_array($body) ? json_encode($body) : $body;
        $timestamp = $timestamp ? $timestamp : time();

        $what = $timestamp.$method.$request_path.$body;

        return base64_encode(hash_hmac("sha256", $what, base64_decode($this->secret), true));
    }
}

function productStringToArr($string)
{
    return explode('-',$string);
}

function startsWith($a, $b) { 
    return strpos($a, $b) === 0;
}
