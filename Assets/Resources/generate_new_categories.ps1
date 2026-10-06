
# Appends synthetic tweet rows for 3 new polarizing categories
# to polarization_tweets.csv

$csvPath = "$PSScriptRoot\polarization_tweets.csv"

# Read existing data to determine last tweet ID
$existing = Get-Content $csvPath
$lastLine = ($existing | Select-Object -Last 1) -split ','
$lastId = [int]($lastLine[0] -replace '\D','')

$users_left  = @("socialist_now","prog_wave","equity_voice","leftist_lens","solidarity_act",
                  "woke_warrior","people_power","dem_socialist","red_rose_99","justice_bloc")
$users_lib   = @("free_markets_","libertad_now","no_tax_no_way","ancap_voice","goldstandard_",
                  "mises_inst","free_thinker","non_aggress","voluntarist_","market_anarch")
$users_id    = @("decolonize_","anti_white_","privilege_watch","critical_race_","blm_lens",
                  "intersect_now","abolish_it_","poc_solidarity","system_smash","white_tear_")

$progressive_tweets = @(
  "Tax the billionaires. Fund the people. Simple math.",
  "Medicare for All isn't radical, it's common sense. #M4A",
  "Union solidarity is the only answer to corporate greed.",
  "Cancel student debt now. Education is a right, not a privilege.",
  "The living wage is not negotiable. $15 is the floor.",
  "Wealth hoarding while people starve is a moral failure.",
  "Corporate lobbying has destroyed democracy. Publicly fund elections.",
  "Universal basic income is the floor, not the ceiling.",
  "No one should go bankrupt for getting sick. #Healthcare",
  "The billionaire class owns the politicians. Wake up.",
  "Rent control saves communities. Landlords extract wealth.",
  "Green jobs can replace fossil fuel jobs. Invest in workers.",
  "Wall Street crashed in 2008, workers paid the price. Never again.",
  "Mutual aid beats charity. Communities over corporations.",
  "A general strike is the only language the bosses understand.",
  "The richest 1% own more than the bottom 90%. This is not democracy.",
  "Universal childcare would transform working families overnight.",
  "Amazon warehouses are modern sweatshops. Unionize everything.",
  "Free public universities exist in dozens of countries. Why not here?",
  "Climate action and economic justice are the same struggle."
)

$libertarian_tweets = @(
  "Taxation is theft. The government has no right to your labor.",
  "The free market solves everything government ruins.",
  "Regulations kill small businesses while protecting corporate monopolies.",
  "End the Federal Reserve. Sound money is the only honest money.",
  "Government healthcare means government control over your body.",
  "Every dollar taxed is a dollar of freedom stolen.",
  "Zoning laws are socialism for neighborhoods. Abolish them.",
  "The war on drugs is a war on personal liberty. End it now.",
  "You own yourself. No state has claim over your choices.",
  "The founding fathers would call today's government tyranny.",
  "Minimum wage laws price the poor out of jobs. Economics 101.",
  "Corporate welfare is just as evil as social welfare. End both.",
  "National security state is the permanent enemy of civil liberties.",
  "Voluntary exchange is the only moral economic system.",
  "The biggest cartel in America is the US government.",
  "Ayn Rand was right. Creators are punished to subsidize looters.",
  "Competing currencies would end inflation overnight.",
  "Carbon taxes are just a new way to control your life.",
  "Property rights are human rights. Stop pretending otherwise.",
  "The deep state and the welfare state are two sides of the same coin."
)

$identitarian_tweets = @(
  "Whiteness is a social construct built on stolen land and labor.",
  "Decolonize your curriculum. Indigenous voices first.",
  "Privilege is invisible to those who have it.",
  "Systemic racism is not opinion, it is policy and history.",
  "The model minority myth is anti-Black racism in disguise.",
  "Colorblindness is just racism with plausible deniability.",
  "Reparations now. 400 years of unpaid labor cannot be ignored.",
  "White feminism centers whiteness and erases Black women. Again.",
  "Police were invented to catch runaway slaves. Abolish them.",
  "Settler colonialism is ongoing. Land back is not a metaphor.",
  "Tone policing is just another form of white supremacy.",
  "Whiteness must be deconstructed before equity is possible.",
  "Implicit bias training is a band-aid on a systemic wound.",
  "Every institution in America was built with Black labor.",
  "The school-to-prison pipeline is a deliberate design choice.",
  "Your silence on racism is a statement. And we hear it.",
  "Diversity without power redistribution is just decoration.",
  "BIPOC communities have been over-policed and under-resourced.",
  "Allyship is action. Performative activism changes nothing.",
  "Anti-racism is not a feeling. It is a daily practice."
)

$progressive_keywords = @("billionaires","tax","healthcare","union","wages","equity","workers",
  "solidarity","medicare","debt","childcare","climate","housing","rights","people","system",
  "corporate","greed","universal","strike","rent","jobs","free","public","justice")

$libertarian_keywords = @("taxation","theft","freedom","markets","government","regulation",
  "liberty","deregulate","federal","reserve","property","rights","voluntary","state","control",
  "welfare","corporations","founding","privacy","sound","money","abolish","compete","individual")

$identitarian_keywords = @("whiteness","systemic","racism","privilege","decolonize","indigenous",
  "reparations","oppression","equity","bias","settler","colonialism","abolish","police","BIPOC",
  "allyship","intersectional","deconstruct","power","anti-racism","solidarity","land","Black","voices")

$categories = @(
  @{name="Progressive Left";     tweets=$progressive_tweets; keywords=$progressive_keywords; users=$users_left},
  @{name="Libertarian Right";    tweets=$libertarian_tweets; keywords=$libertarian_keywords; users=$users_lib},
  @{name="Identitarian Left";    tweets=$identitarian_tweets; keywords=$identitarian_keywords; users=$users_id}
)

$newRows = [System.Collections.Generic.List[string]]::new()
$id = $lastId + 1

foreach ($cat in $categories) {
  for ($i = 0; $i -lt 1000; $i++) {
    $tweetIdx = Get-Random -Minimum 0 -Maximum $cat.tweets.Count
    $tweet    = $cat.tweets[$tweetIdx] -replace '"','""'   # escape quotes
    $user     = $cat.users[(Get-Random -Minimum 0 -Maximum $cat.users.Count)] + (Get-Random -Minimum 1000 -Maximum 9999)
    
    # Pick 4-6 keywords
    $kwCount  = Get-Random -Minimum 4 -Maximum 7
    $shuffled = $cat.keywords | Get-Random -Count $kwCount
    $kwStr    = $shuffled -join ', '

    $likes    = Get-Random -Minimum 100  -Maximum 15000
    $rts      = Get-Random -Minimum 50   -Maximum 8000
    $replies  = Get-Random -Minimum 10   -Maximum 3000
    $sent     = [math]::Round((Get-Random -Minimum -100 -Maximum -30) / 100.0, 3)
    $tox      = [math]::Round((Get-Random -Minimum 30  -Maximum 90)  / 100.0, 3)

    $timestamp = "2023-{0:D2}-{1:D2} {2:D2}:{3:D2}:00" -f `
      (Get-Random -Minimum 1 -Maximum 13), `
      (Get-Random -Minimum 1 -Maximum 29), `
      (Get-Random -Minimum 0 -Maximum 24), `
      (Get-Random -Minimum 0 -Maximum 60)

    $idStr = "tw_{0:D6}" -f $id
    $row = "$idStr,$user,$timestamp,$($cat.name),polarizing,`"$tweet`",$likes,$rts,$replies,$sent,$tox,`"$kwStr`""
    $newRows.Add($row)
    $id++
  }
}

# Append to CSV (no BOM, same encoding)
$newRows | Out-File -FilePath $csvPath -Encoding utf8 -Append
Write-Host "Done! Appended $($newRows.Count) rows."
