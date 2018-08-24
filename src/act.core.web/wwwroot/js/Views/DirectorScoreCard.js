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
$.onAction('loadScoreCard',
    function (e, data) {
        if (data.type) {
            Lockr.prefix = data.type;
        }
        var $t = $(this).on('reloadscorecard',
            function (e2, url, hash, name) {
                var $t = $(this).html($('<h3/>').addInlineLoading(' loading...'));
                jay.ajaxPostHtml({
                    url: url
                }).done(function (html) {
                    var $html = $(html);
                    var hdata = $html.data();
                    if (hdata.empty === true) {
                        $t.empty().trigger('scorecardempty', [hdata]);
                    } else {
                        $t.html($html).trigger('scorecardloaded', [hdata]);
                    }
                    if (data.type) {
                        location.hash = hash;
                        Lockr.set(hash,
                            {
                                url: url,
                                name: name
                            });
                        if (data.searchBox) {
                            $(data.searchBox).val(name);
                        }
                    }
                }).fail(function () {
                    $t.empty().message('error', jay.failMessage);
                });
            });
        var url = data.url;
        var hash = data.hash;
        var lhash = location.hash.replace('#', '');
        var name = data.name;
        if (data.type && lhash) {
            var ldata = Lockr.get(lhash);
            if (ldata && ldata.url && ldata.name) {
                url = ldata.url;
                name = ldata.name;
                hash = lhash;
            }
        }
        $t.trigger('reloadscorecard', [url, hash, name]);
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
