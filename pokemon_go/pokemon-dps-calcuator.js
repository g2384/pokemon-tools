function calculateCP(atk, def, sta, level) {
    return Math.floor(Math.max(10, (atk * (def ** 0.5) * (sta ** 0.5) * data_cp_multiplier.find(item => item.level === level).multiplier ** 2) / 10))
}

function calculateStatsByTag(baseStats, tag) {
    let checkNerf = tag && tag.toLowerCase().includes("mega") ? false : true;
    const atk = calBaseATK(baseStats, checkNerf);
    const def = calBaseDEF(baseStats, checkNerf);
    const sta = tag !== "shedinja" ? calBaseSTA(baseStats, checkNerf) : 1;
    return {
        atk: atk,
        def: def,
        sta: sta,
    };
}

function calBaseATK(stats, nerf) {
    const atk = stats.atk ?? stats.find(item => item.stat.name === "attack").base_stat;
    const spa = stats.spa ?? stats.find(item => item.stat.name === "special-attack").base_stat;

    const lower = Math.min(atk, spa);
    const higher = Math.max(atk, spa);

    const speed = stats.spe ?? stats.find(item => item.stat.name === "speed").base_stat;

    const scaleATK = Math.round(2 * ((7 / 8) * higher + (1 / 8) * lower));
    const speedMod = 1 + (speed - 75) / 500;
    const baseATK = Math.round(scaleATK * speedMod);
    if (!nerf) return baseATK;
    if (calculateCP(baseATK + 15, calBaseDEF(stats, false) + 15, calBaseSTA(stats, false) + 15, 40) >= 4000) return Math.round(scaleATK * speedMod * 0.91);
    else return baseATK;
}

function calBaseDEF(stats, nerf) {
    const def = stats.def ?? stats.find(item => item.stat.name === "defense").base_stat;
    const spd = stats.spd ?? stats.find(item => item.stat.name === "special-defense").base_stat;

    const lower = Math.min(def, spd);
    const higher = Math.max(def, spd);

    const speed = stats.spe ?? stats.find(item => item.stat.name === "speed").base_stat;

    const scaleDEF = Math.round(2 * ((5 / 8) * higher + (3 / 8) * lower));
    const speedMod = 1 + (speed - 75) / 500;
    const baseDEF = Math.round(scaleDEF * speedMod);
    if (!nerf) return baseDEF;
    if (calculateCP(calBaseATK(stats, false) + 15, baseDEF + 15, calBaseSTA(stats, false) + 15, 40) >= 4000) return Math.round(scaleDEF * speedMod * 0.91);
    else return baseDEF;
}

function calBaseSTA(stats, nerf) {
    const hp = stats.hp ?? stats.find(item => item.stat.name === "hp").base_stat;

    const baseSTA = Math.floor(hp * 1.75 + 50);
    if (!nerf) return baseSTA;
    if (calculateCP(calBaseATK(stats, false) + 15, calBaseDEF(stats, false) + 15, baseSTA + 15, 40) >= 4000) return Math.round((hp * 1.75 + 50) * 0.91);
    else return baseSTA;
}

function SortDps(dps_table, newPokemon, excludeLegendary, includeReleasedOnly, filterTypes, enabledFilterTypes, filterMoveTypes, enabledMoveTypes, excludePokemons) {
    var allMovesArray = [];
    var pokeCount = {};
    for (var [key, value] of Object.entries(dps_table)) {
        var pok = newPokemon[key];
        if (!(key in pokeCount)) {
            pokeCount[key] = -1;
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
        else if (excludePokemons.includes(dps_table[key].name.toLowerCase())) {
            continue;
        }
        var ms = value.all_moves.sort(function (e, t) {
            return t[6] - e[6] // DPS^3*TDO
        });

        for (var v2 of ms) {
            pokeCount[key]++;
            if (filterMoveTypes
                && !enabledMoveTypes.includes(v2[1])
                && !enabledMoveTypes.includes(v2[3])) {
                continue;
            }

            allMovesArray.push([value.id, key, v2[0], v2[1], v2[2], v2[3], v2[4], v2[5], v2[6], pokeCount[key], v2[7], v2[8]])
        }
    }
    return allMovesArray;
}

function getImageId(id, name) {
    if (id in pokemonGo_Assets_ImagesConvert) {
        return String(pokemonGo_Assets_ImagesConvert[id])
    }
    else if (name.toLowerCase().indexOf("alola") >= 0) {
        return "61"
    }
    else {
        return "00"
    }
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
        var highlight = false;
        for (var h of highlightMatched) {
            highlight = value.includes(h)
            if (highlight) {
                break;
            }
        }
        var releasedTag = pok.releasedGO ? "" : "<span class='not-released roundSpan'>N/A</span>"
        var highlight2 = highlight ? "highlight" : "";
        var vm = "<tr><td class='" + highlight2 + "' rowspan='" + value.length + "'>" + counter
            + "</td><td class='pokemon-name' rowspan='" + value.length + "'><img src='https://raw.githubusercontent.com/PokeMiners/pogo_assets/master/Images/Pokemon/pokemon_icon_"
            + String(id).padStart(3, '0') + "_" + getImageId(id, dps_table[value[0][1]].name)
            + ".png' /><span class='inline'>"
            + dps_table[value[0][1]].name + "</span>"
            + pok.types.map(x => "<span class='inline roundSpan type-" + x + "'>" + x + "</span>").join("") + releasedTag + "</td>";
        vm += value.map(x => {
            var rk_move = x[9];
            var rank = rk_move < rankChars.length ? rankChars[rk_move] : " (" + (rk_move + 1) + ")";
            rankedMoves[id]++;
            var quickMoveTag = x[10] != "Normal" ? "<span class='move-tag " + x[10].toLowerCase() + "-move'>" + x[10] + "</span>" : "";
            var chargeMoveTag = x[11] != "Normal" ? "<span class='move-tag " + x[11].toLowerCase() + "-move'>" + x[11] + "</span>" : "";
            return "<td><img src='./pokemon_go/type_" + x[3] + ".png' />" + x[2] + quickMoveTag + "</td><td><img src='./pokemon_go/type_" + x[5] + ".png' />"
                + x[4] + chargeMoveTag + "</td><td>"
                + numberWithCommas(x[6], 2) + "</td><td>"
                + numberWithCommas(x[7], 2) + "</td><td>"
                + numberWithCommas(x[8], 2) + "<span>" + rank + "</span>" + "</td>";
        }).join("</tr><tr>") + "</tr>";
        str += vm;
    }
    str += "</tbody></table>";
    return str;
}