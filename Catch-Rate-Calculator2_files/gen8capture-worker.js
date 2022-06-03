var status_mods = {
   none: 1,
   psn: 1.5,
   par: 1.5,
   brn: 1.5,
   slp: 2.5,
   frz: 2.5
};
var gvals = [1229/4096, 0.5, 2867/4096, 3277/4096, 3686/4096, 1];
var pvals = [0, 0.5, 1, 1.5, 2, 2.5];

function isUltraBeast(pokemon) {
   return pokemon >= 793 && pokemon <= 799 || pokemon >= 803 && pokemon <= 806;
}

function inArray(item, array) {
   for (var i = 0; i < array.length; i++) {
      if (array[i] === item) {
         return true;
      }
   }
   return false;
}

function getBallBonus(event) {
   // If it's an Ultra Beast, then everything except Beast Balls and Master Balls has a ball bonus of 0.1x (410/4096).
   if (isUltraBeast(event.pokemon.id) && event.ball !== 'beast-ball' && event.ball !== 'master-ball') {
      return 410/4096;
   }

   switch (event.ball) {
      case 'great-ball':
      case 'safari-ball':
         return 1.5;
      case 'ultra-ball':
         return 2;
      case 'master-ball':
         return -1;
      case 'net-ball':
         if (inArray('bug', event.pokemon.types) || inArray('water', event.pokemon.types)) {
            return 3.5;
         }
         else {
            return 1;
         }
      case 'nest-ball':
         if (event.level < 31) {
            return Math.max(1, (41 - event.level) / 10);
         } else {
            return 1;
         }
      case 'dive-ball':
         if (event.underwater) {
            return 3.5;
         }
         else {
            return 1;
         }
      case 'repeat-ball':
         if (event.registered) {
            return 3.5;
         }
         else {
            return 1;
         }
      case 'timer-ball':
         return Math.min(4, 1 + (event.turn - 1) * 1229/4096);
      case 'quick-ball':
         if (event.turn === 1) {
            return 5;
         }
         else {
            return 1;
         }
      case 'dusk-ball':
         if (event.dark) {
            return 3;
         }
         else {
            return 1;
         }
      case 'fast-ball':
         if (event.pokemon.speed >= 100) {
            return 4;
         }
         else {
            return 1;
         }
      case 'level-ball':
         if (Math.floor(event.your_level / 4) > event.level) {
            return 8;
         }
         else if (Math.floor(event.your_level / 2) > event.level) {
            return 4;
         }
         else if (event.your_level > event.level) {
            return 2;
         }
         else {
            return 1;
         }
      case 'love-ball':
         if (event.your_pokemon === event.pokemon.id && (event.gender === "male" && event.your_gender === "female" || event.gender === "female" && event.your_gender === "male")) {
            return 8;
         }
         else {
            return 1;
         }
      case 'lure-ball':
         if (event.fishing) {
            return 4;
         }
         else {
            return 1;
         }
      case 'moon-ball':
         if (event.pokemon.id >= 29 && event.pokemon.id <= 36 || event.pokemon.id === 39 || event.pokemon.id === 40 || event.pokemon.id === 300 || event.pokemon.id === 301 || event.pokemon.id === 517 || event.pokemon.id === 518) {
            return 4;
         }
         else {
            return 1;
         }
      case 'beast-ball':
         if (isUltraBeast(event.pokemon.id)) {
            return 5;
         }
         else {
            return 410/4096;
         }
      case 'dream-ball':
         if (event.status === 'slp' || event.pokemon.id === 775) { // Komala, which has the Comatose ability
            return 4;
         }
      default:
         return 1;
   }
}

function getDexCat(event) {
   var dexcat = 0;
   if (event.pokedex > 30) {
      dexcat++;
   }
   if (event.pokedex > 150) {
      dexcat++;
   }
   if (event.pokedex > 300) {
      dexcat++;
   }
   if (event.pokedex > 450) {
      dexcat++;
   }
   if (event.pokedex > 600) {
      dexcat++;
   }
   return dexcat;
}

function down(x) {
   // Rounds down to the nearest 1/4096th
   return Math.floor(x * 4096) / 4096;
}

function round(x) {
   // Rounds to the nearest 1/4096th
   return Math.round(x * 4096) / 4096;
}

function calcX(event) {
   var c = event.pokemon.catch_rate;

   if (event.ball === 'heavy-ball') {
      var weight = event.pokemon.weight;
      if (weight >= 3000) {
         c += 30;
      }
      else if (weight >= 2000) {
         c += 20;
      }
      else if (weight < 1000) {
         c -= 20;
      }
   }
   c = Math.max(c, 1);

   var b = getBallBonus(event);
   if (b === -1) {
      return 256;
   }
   var s = event.raid ? 1 : status_mods[event.status];
   var m = event.max_hp;
   var h = event.raid ? 1 : event.cur_hp;

   var g = !event.raid && event.thickgrass ? gvals[getDexCat(event)] : 1;
   var l = 10;
   if (event.level < 21) {
      l = (30 - event.level);
   }
   var d = event.raid ? event.raid_multiplier * 4096 : event.postgame ? 4096 : event.your_level < event.level ? 410 : 4096;

   console.log(c, b, s, m, h, d);

   return Math.min(255, round(round(down(down(round(round((3 * m - 2 * h) * g) * c * b) / (3 * m)) * l / 10) * s) * d / 4096));
}

function calculateResults(event) {
   var x = calcX(event);
   if (x >= 256) {
      // Auto-catch
      return {
         success: 1,
         wobbles: [0, 0, 0, 0]
      };
   }

   console.log("Calculated X:", x);

   // Using the full formula for Y
   var y = x === 0 ? 0 : Math.floor(round(65536 / round(Math.pow(round(255 / x), 3 / 16))));
   console.log("Calculated Y:", y);
   var y_chance = y / 65536;
   if (y_chance > 1) {
      // A chance greater than 100% is nonsensical, so bump it back down
      y_chance = 1;
   }
   var y_compl = 1 - y_chance;

   // Check for a critical capture
   var p = pvals[getDexCat(event)];
   var charm = event.catching_charm ? 2 : 1;
   var cc = event.raid ? 0 : Math.floor(round(Math.min(x, 255) * p * charm) / 6);

   var crit_chance = Math.min(1, cc / 256); // Can't have a better than 100% chance of a critical capture...

   var success_chance = Math.pow(y_chance, 4);
   return {
      success: success_chance,
      wobbles: [
         y_compl,
         y_chance * y_compl,
         Math.pow(y_chance, 2) * y_compl,
         Math.pow(y_chance, 3) * y_compl
      ],
      critical: crit_chance,
      crit_success: y_chance
   };
}

onmessage = function(event) {
   console.log("Worker received message", event.data);
   postMessage(calculateResults(event.data));
   close();
}