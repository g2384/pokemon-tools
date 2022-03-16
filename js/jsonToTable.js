function buildHtmlTable(arr, order, htmlColumns) {
    var table = _table_.cloneNode(false),
        columns = addAllColumnHeaders(arr, order, table);
    var tbody = document.createElement('tbody');
    for (var i = 0, maxi = arr.length; i < maxi; ++i) {
        var tr = _tr_.cloneNode(false);
        for (var j = 0, maxj = columns.length; j < maxj; ++j) {
            var td = _td_.cloneNode(false);
            var cellValue = arr[i][columns[j]] || '';
            var d;
            if (htmlColumns.indexOf(columns[j]) >= 0) {
                d = document.createElement('span');
                d.innerHTML = cellValue;
            }
            else {
                d = document.createTextNode(cellValue);
            }
            td.appendChild(d);
            tr.appendChild(td);
        }
        tbody.appendChild(tr);
    }
    table.appendChild(tbody);
    return table;
}

function addAllColumnHeaders(arr, order, table) {
    var columnSet = [],
        tr = _tr_.cloneNode(false);
    var thead = document.createElement('thead');
    for (var i = 0, l = arr.length; i < l; i++) {
        for (var key in arr[i]) {
            if (arr[i].hasOwnProperty(key) && columnSet.indexOf(key) === -1) {
                columnSet.push(key);
            }
        }
    }
    if (order != null && order != undefined) {
        var cs = [];
        for (var i = 0; i < order.length; i++) {
            var ind = columnSet.indexOf(order[i]);
            if (ind >= 0) {
                cs.push(columnSet[ind]);
                columnSet.splice(ind, 1);
            }
        }
        columnSet = cs.concat(columnSet);
    }
    for (var i = 0; i < columnSet.length; i++) {
        var key = columnSet[i];
        var th = _th_.cloneNode(false);
        th.appendChild(document.createTextNode(key));
        tr.appendChild(th);
    }
    thead.appendChild(tr);
    table.appendChild(thead);
    return columnSet;
}