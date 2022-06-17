function SortDps(dps_table, newPokemon, excludeLegendary, includeReleasedOnly, filterTypes, enabledFilterTypes, filterMoveTypes, enabledMoveTypes, excludepokemons){
    var allMovesArray = [];
    var pokeCount = {};
    for (var [key, value] of Object.entries(dps_table)) {
        var pok = newPokemon[key];
        if(!(key in pokeCount)){
            pokeCount[key] = 0;
        }
        if (excludeLegendary && pok.pokemonClass == "LEGENDARY") {
            continue;
        }
        else if (includeReleasedOnly && !pok.releasedGO) {
            continue;
        }
        else if (pok.forme == "Mega") {
            continue;
        }
        else if (filterTypes && !pok.types.includesAny(enabledFilterTypes)) {
            continue;
        }
        else if (excludepokemons.includes(dps_table[key].name.toLowerCase())) {
            continue;
        }
        var ms = value.all_moves.sort(function (e, t) {
            return t[6] - e[6] // DPS^3*TDO
        });

        for (var v2 of ms) {
            if (filterMoveTypes
                && !enabledMoveTypes.includes(v2[1])
                && !enabledMoveTypes.includes(v2[3])) {
                continue;
            }
            
            
            allMovesArray.push([value.id, key, v2[0], v2[1], v2[2], v2[3], v2[4], v2[5], v2[6], pokeCount[key]])
            pokeCount[key]++;
        }
    }
    return allMovesArray;
}