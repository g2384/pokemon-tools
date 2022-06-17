function SortDps(dps_table, newPokemon, excludeLegendary, includeReleasedOnly, filterTypes, enabledFilterTypes, filterMoveTypes, enabledMoveTypes, excludepokemons) {
    var allMovesArray = [];
    var pokeCount = {};
    for (var [key, value] of Object.entries(dps_table)) {
        var pok = newPokemon[key];
        if (!(key in pokeCount)) {
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

function GenerateDpsAttackTable(allMovesArray, showTop, dps_table, newPokemon, pokemonGo_Assets_ImagesConvert, highlightMatched) {
    var group = {};
    var pre_value = -1;
    var counter = 0;
    for (var value of allMovesArray) {
        if (pre_value != value[0]) {
            counter++;
            group[counter] = [value]
            pre_value = value[0];
        }
        else {
            group[counter].push(value);
        }
        if (showTop > 0 && counter >= showTop) {
            break;
        }
    }

    var str = "<table class='table table-striped align-middle'><thead><tr><th></th><th>Pokemon</th><th>Fast Move</th><th>Charge Move</th><th>DPS</th><th>TDO</th><th>DPS^3*TDO</th></tr></thead><tbody>";
    counter = 0;
    var rankedMoves = {};
    const rankChars = ["&#9733;", "&#10103;", "&#10104;"];
    if (highlightMatched == undefined || highlightMatched == null) {
        highlightMatched = [];
    }
    for (const [key, value] of Object.entries(group)) {
        var id = dps_table[value[0][1]].id;
        var pok = newPokemon[value[0][1]];
        counter++;
        if (!(id in rankedMoves)) {
            rankedMoves[id] = 0;
        }
        var rank = rankedMoves[id] < 3 ? rankChars[rankedMoves[id]] : " (" + (rankedMoves[id] + 1) + ")";
        rankedMoves[id]++;
        var highlight = false;
        for (var h of highlightMatched) {
            highlight = value.includes(h)
            if (highlight) {
                break;
            }
        }
        var highlight2 = highlight ? "highlight" : "";
        var vm = "<tr><td class='" + highlight2 + "' rowspan='" + value.length + "'>" + counter
            + "</td><td class='pokemon-name' rowspan='" + value.length + "'><img src='https://raw.githubusercontent.com/PokeMiners/pogo_assets/master/Images/Pokemon/pokemon_icon_"
            + String(id).padStart(3, '0') + "_" + ((id in pokemonGo_Assets_ImagesConvert) ? String(pokemonGo_Assets_ImagesConvert[id]) : "00")
            + ".png' /><span class='inline'>"
            + dps_table[value[0][1]].name + "</span>"
            + pok.types.map(x => "<span class='inline roundSpan type-" + x + "'>" + x + "</span>").join("") + "</td>";
        vm += value.map(x =>
            "<td><img src='./pokemon_go/type_" + x[3] + ".png' />" + x[2] + "</td><td><img src='./pokemon_go/type_" + x[5] + ".png' />"
            + x[4] + "</td><td>"
            + numberWithCommas(x[6], 2) + "</td><td>"
            + numberWithCommas(x[7], 2) + "</td><td>"
            + numberWithCommas(x[8], 2) + "<span>" + rank + "</span>" + "</td>").join("</tr><tr>") + "</tr>";
        str += vm;
    }
    str += "</tbody></table>";
    return str;
}