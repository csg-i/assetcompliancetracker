(function($) {
    var minus = 'fa-chevron-up';
    var plus = 'fa-chevron-down';
    var alltypes = 'div.panel-body, ul.list-group, table.table, div.list-group';
    $.onAction('load',
        function(e, data) {
            var $t = $(this).addInlineLoading();
            jay.ajaxPostHtml({ url: data.url })
                .done(function(html) {
                    var $html = $(html);
                    $html.find('div.card div.card-header')
                        .append($('<i/>')
                            .addClass('fa fa-lg float-right')
                            .addClass(plus)
                        )
                        .css('cursor', 'pointer')
                        .addClass('action-toggle');
                    $html.find('div.panel').find(alltypes).hide();
                    $t.html($html);
                }).fail(function() {
                    $t.empty().message('error', jay.failMessage);
                });
        });
    $.onAction('toggle',
        function() {
            var $t = $(this).find('i.fa');
            var action;
            if ($t.hasClass(plus)) {
                $t.removeClass(plus).addClass(minus);
                action = 'slideDown';
            } else {
                $t.removeClass(minus).addClass(plus);
                action = 'slideUp';
            }
            $t.closest('div.panel').find(alltypes)[action]('slow');
        });

})(jQuery);