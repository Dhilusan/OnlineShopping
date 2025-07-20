var dataTable;
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else {
        if (url.includes("completed")) {
            loadDataTable("completed");
        }
        else {
            if (url.includes("approved")) {
                loadDataTable("approved");
            }
            else {
                if (url.includes("cancelled")) {
                    loadDataTable("cancelled");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }


});


function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/order/getall?status=' + status },
        "columns": [
            { data: 'orderStatus', "width": "50%" }, // Show only the orderStatus column
            { data: 'orderTotal', "width": "50%", "render": $.fn.dataTable.render.number(',', '.', 2, 'RS.') }, // Show orderTotal and format as currency
        ],
        "footerCallback": function (row, data, start, end, display) {
            var api = this.api();
            var total = api
                .column(1, { page: 'current' })
                .data()
                .reduce(function (acc, val) {
                    return acc + parseFloat(val);
                }, 0);

            $(api.column(1).footer()).html('Total: ' + total); // Display the total in the footer for orderTotal column
        }
    });
}



