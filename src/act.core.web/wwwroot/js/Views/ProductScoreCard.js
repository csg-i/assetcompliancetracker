$.onDataEvent('totalTable',
    'table',
    function (v) {
        $(this)[v === 'true' ? 'addClass' : 'removeClass']('total');
    });
$.onDataEvent('filter',
    'table tbody',
    function (v) {
        $(this).filter(v.total, v.filter);
    });
$.onDataEvent('errorMessage',
    '#messageArea',
    function (d) {
        $(this).message('error', d);
    });
$.onAction('totalOrPci',
    function() {
        var val = $(this).closest('div[role="form"]').find(':radio').getInputValues();
        $.triggerDataEvent({
            totalTable: val.total,
            filter: val
        });
    });
$.onAction('filter',
    function () {
        var val = $(this).closest('div[role="form"]').find(':radio').getInputValues();
        $.triggerDataEvent('filter', val);
    });
$.onAction('loadAll',
    function (e, data) {
        var $this = $(this);
        jay.ajax({
            url: data.url
        }).done(function (json) {
            var urls = json.urls || [];
            var ln = urls.length;           
            for (var i = 0; i < ln; i++) {
                var url = urls[i];
                jay.ajaxPostHtml({
                    url: url
                }).done(function (html) {
                    var val = $(':radio').getInputValues();
                    var $html = $(html);
                    var hData = $html.data();
                    if (hData.empty !== true) {
                        $html.find('tbody').filter(val.total, val.filter);
                        $this.append($html.find('tbody tr'));
                    }
                }).fail(function() {
                    $.triggerDataEvent('errorMessage', jay.failMessage);
                });
            }
        }).fail(function () {
            $.triggerDataEvent('errorMessage', jay.failMessage);
        });
    });

$.fn.filter = function(total, filter) {
    return this.each(function () {
        var $t = $(this);
        if (filter === '') {
            var countQuery = '[data-' + (total === 'true' ? 'all-' : 'pci-') + 'count="0"]';
            $t.find('tr:not(' + countQuery + ')').show();
            $t.find('tr' + countQuery).hide();
        } else {
            var query = '[data-' + (total === 'true' ? 'all-' : 'pci-') + filter + '="0"]';
            $t.find('tr:not(' + query + ')').show();
            $t.find('tr' + query).hide();
        }
    });
};
$(function(){
    $('div[data-visibility-action="loadScoreCard"]')
        .on('scorecardempty',
            function() {
                $(this).message('info', 'No score card data.');
            })
        .on('scorecardloaded',
            function(e, data) {
                var val = $('div[role="form"]').find(':radio').getInputValues();
                $.triggerDataEvent({
                    totalTable: val.total,
                    filter: val
                });
            });
});
