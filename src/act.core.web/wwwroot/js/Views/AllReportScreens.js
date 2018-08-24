$.onAction('loadReport',
    function (e, data) {
        var $t = $(this).html($('<h2/>').addInlineLoading('loading...'));
        jay.ajaxPostHtml({
            url: data.url
        }).done(function (html) {
            $t.html(html);
        }).fail(function () {
            $t.empty().message('error', jay.failMessage);
        });
    });

$.onDataEvent('totalTable',
    'table',
    function (v) {
        $(this)[v === 'true' ? 'addClass' : 'removeClass']('total');
    });
$.onAction('totalOrPci',
    function () {
        var val = $(this).closest('div[role="form"]').find(':radio').getInputValues();
        $.triggerDataEvent({
            totalTable: val.total
        });
    });