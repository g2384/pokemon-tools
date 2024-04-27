# Pokemon Tools

## BDSP

[Daily Event Calendar for BDSP, including Feebas Finder](https://g2384.github.io/pokemon-tools/BDSP-Daily-Event-Calendar.html)

## Gen VIII

[Gen VIII Catch Rate Calculator](https://g2384.github.io/pokemon-tools/Catch-Rate-Calculator.html)

## Pokemon Go

[Pokemon Go Tools](https://g2384.github.io/pokemon-tools/Pokemon-Go.html)

- IV Appraisal Stats Chart
- Find IV stats from CP
- Top Attackers

- `cp_multiplier.json` from https://pogoapi.net/api/v1/cp_multiplier.json
- `moves_pokemon_list.json` from https://pogoapi.net/api/v1/current_pokemon_moves.json

### Evolution:

1. get latest.json from https://raw.githubusercontent.com/PokeMiners/game_masters/master/latest/latest.json
   1. or https://github.com/PokeMiners/game_masters/blob/master/latest/latest.json
2. run `pokemon_go/calculator_evolution.html`
3. copy console to `latest_evolution_quest.json`
4. `pokemon_evolutions.json` is from https://pogoapi.net/api/v1/pokemon_evolutions.json. `latest.json["cpMultiplier"]` doesn't have `0.5` levels

## Pokedex

[Pokedex](https://g2384.github.io/pokemon-tools/pokedex.html)

### Icons

- icon_trade.png: https://github.com/PokeMiners/pogo_assets/blob/master/Images/Friends/trade_icon_small.png
- icon_mythic.png: https://github.com/PokeMiners/pogo_assets/blob/master/Images/Filters/ic_mythical.png
- icon_candy.png: https://github.com/PokeMiners/pogo_assets/blob/master/Images/Menu%20Icons/ic_candy.png
- icon_quest.png: https://github.com/PokeMiners/pogo_assets/blob/master/Images/Menu%20Icons/QuestStar.png
- icon_deploy.png: https://github.com/PokeMiners/pogo_assets/blob/master/Images/Pokestops%20and%20Gyms/ic_deploy.png
- icon_shadow.png: https://github.com/PokeMiners/pogo_assets/blob/master/Images/Rocket/ic_shadow.png
- icon_mega.png: https://github.com/PokeMiners/pogo_assets/blob/master/Images/Megas%20and%20Primals/pokemon_details_cp_mega.png
- icon_primal.png: https://github.com/PokeMiners/pogo_assets/blob/master/Images/Raids/ActivityLogPrimalRaidLogo.png

### Best types

- best_per_type.json: https://db.pokemongohub.net/pokemon-list/best-per-type/ -> `counters` Fetch/XHR

### CP calculator:

https://pokemon.gameinfo.io/en/tools/cp-calculator

### Best PVP team:

open Team_Builder.html, run `InterfaceMaster.getInstance().SearchBestTeam()`
PVP rankings: `https://pvpoke.com/rankings/jungle/1500/overall/steelix/` -> jungle.json
raw data: https://github.com/pvpoke/pvpoke/blob/master/src/data/rankings/all/overall/rankings-1500.json
