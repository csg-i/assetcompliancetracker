$.onDataEvent('filter',
    'table tbody',
    function (data) {
        
    });

$.onAction('zeroData',
    function() {
        var val = $(this).val();
        $(document).trigger('applyFilter', ['className', val]);
    });

$.onAction('goto',
    function (e, data) {
        Lockr.set(data.lockr, {search: data.name, searchType: 'OsSpec'});
        window.location.href = data.url;
    });

$.onAction('filter',
    function (e, data){
        var val = $(this).val();
        $(document).trigger('applyFilter', ['filter', val]);
    });

$.onAction('clearFilter',
    function (){
        $(this).closest('div.input-group').find(':input.form-control').val('').doAction('filter').focus();
    });

$(document)
    .data('filterLogic', {
        filter: '',
        className: ''
    })
    .on('applyFilter', function (e, type, value) {
        var $t = $(this);
        var data = $t.data('filterLogic');
        data[type] = value;
        $t.data('filterLogic', data);
        
        var $tbody = $('table tbody');
       
        
        if (data.className && data.filter){
            $('tr:not(.' + data.className + ')', $tbody).hide();
            $('tr.' + data.className, $tbody).hide(function () {
                $(this).dataFilter('filter', data.filter.toLowerCase(), '*=').show();
            });
            
        } else if (data.className){
            $('tr:not(.' + data.className + ')', $tbody).hide();
            $('tr.' + data.className, $tbody).show();
        } else if (data.filter){
            $('tr', $tbody).hide(function () {
                $(this).dataFilter('filter', data.filter.toLowerCase(), '*=').show();
            });
        } else{
            $('tr', $tbody).show();
        }
    });