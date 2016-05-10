param(  
		$jurl        = 'http://localhost:55625/Rai.Media.Web.Services/Json/Rest.aspx'
      , $asset       = $null
      , $interactive = $true
      , $preurl      = 'http://localhost:7777/dash'
	  , $mpd         = 'pr.mpd'
	  , $outfile     = $null
	  , [DateTime] $mindate	 = '1/1/1899'
	  , [DateTime] $maxdate	 = [DateTime]::Now.Add([TimeSpan]::FromDays(1))
)

$ErrorActionPreference = "Stop";

$my_dir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

if($null -eq $asset)
{
	Write-Error "Asset Not Provided";
}

if($null -eq $outfile)
{
	$outfile = "$($my_dir)\Media.xml";

}

$outfile | oh

if(test-path $outfile)
	{rm $outfile}


function time-format()
{
	$fd = "yyyy-MM-ddTHHmmssfffZ";
	[System.Globalization.DateTimeFormatInfo] $df = new-object System.Globalization.DateTimeFormatInfo
	$df.FullDateTimePattern = $fd

	return $df;
}

function parse-dt($dt)
{
	$df = time-format
	return [System.DateTime]::ParseExact($dt, "F", $df, 'AssumeUniversal');
}

function to-dt([DateTime] $date)
{
	$df = time-format
	return $date.ToUniversalTime().ToString("F", $df);
}


function curl-json($url)
{
 try {
        $request = Invoke-WebRequest -Uri $url -ContentType "application/json" #-ea SilentlyContinue 
    } 
    catch [System.Net.WebException] {
        $request = $_.Exception.Response

    }

    return $request;
	
}

function hh($tt)
{
  return [TimeSpan]::FromTicks($tt).ToString();
}




function do-asset-leg($al, $file)
{
	$al | oh

	if(-Not ($al.ContentType -match '(mp4)|(h264)'))
	{
		"excluded: $($al.ContentType)" | oh
		return;
	}

	
	

	$start = parse-dt $al.StartTime;
	$end   = parse-dt $al.EndTime;



	if($start -lt $mindate -and $end -lt $mindate)
	{
		"out-date $start [$end] - $mindate" | oh
		return;
	}

	"<!--=============$($al.ContentType)===============-->" | out-file $file -append


	if($start -lt $mindate)
	{
		$start = $mindate;
	}

	$rnd   = Get-Random -minimum 1 -maximum 241
	$rl    = Get-Random -minimum 60 -maximum 7201

	$step  = [TimeSpan]::FromSeconds($rnd);
	$play_len = [TimeSpan]::FromSeconds($rl);

	$len   = $end.Subtract($start);

	

	while($start -lt $end)
	{
		$play_end = $start.Add($play_len);

		if($end -lt $play_end)
		{  $play_end = $end;}

		$time_range = "$(to-dt $start)/$(to-dt $play_end)";

		$url = "/$($al.Name)/$($time_range)/$($al.ContentType.Replace(' ', '%20'))";

		'<Url>'  | out-file $file -append
		#"	<full>$($preurl)$($url)/$mpd</full>"  | out-file $file -append
		"   <prefix>$($preurl)$($url)</prefix>"  | out-file $file -append
		'</Url>'  | out-file $file -append
		
		$url | oh

		$start = $start.Add($step);

		$rnd   = Get-Random -minimum 1 -maximum 241
		$rl    = Get-Random -minimum 60 -maximum 7201

		$step	  = [TimeSpan]::FromSeconds($rnd);
		$play_len = [TimeSpan]::FromMinutes($rl);
	}


	
}

function do-asset($asset)
{
	"===============$asset===================" | oh
	
	$r = curl-json "$jurl/info/$asset"
	
	if(200 -ne $r.StatusCode)
	{
		"MISSING ASSET $($r.StatusCode)" | oh
		return;

		$r.Content | oh
		Write-Error "Request Failed $($r.StatusCode)";
		return;
	}

	$json = ConvertFrom-Json $r.Content;
	
	$file = "$outfile";
	
	

	$json.AssetInfo | foreach {do-asset-leg $_ $file};

	


}

function main()
{
	$target = '';
	if($null -ne $asset)
	{
		$target = "/$asset";
	}
	$r = curl "$jurl/asset$($target)" -ContentType "application/json"

	if(200 -ne $r.StatusCode)
	{
		$r.StatusCode | oh
		$r.Content | oh
		Write-Error "Request Failed " + $r.StatusCode;
		return;
	}

	$json = ConvertFrom-Json $r.Content;

	$json | oh

	"<Urls>" | out-file $outfile -append
	
	$json.AssetList | foreach{do-asset $_} 

	"</Urls>" | out-file $outfile -append
}


main


