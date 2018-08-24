$.onDataEvent('changeCount',
    'span.count',
    function (val) {
        $(this).html(val);
    });
$.onDataEvent('changeSearchType',
    'span.search-type',
    function(val) {
        $(this).html(val);
    });
$.onDataEvent('hideJumbotron',
    'div.jumbotron',
    function (hide) {
        $(this)[hide ? 'addClass' : 'removeClass']('d-none');
    });

$.onDataEvent('changeSearchPlaceHolder',
    'input[name="search"]',
    function (data) {
        $(this).data('searchType', data.type).attr('placeholder', data.placeHolder).val('').doAction('search');
    });
$.onDataEvent('clearResults',
    '#results',
    function() {
        $(this).empty();
    });
$.onDataEvent('search',
    '#results',
    function (data) {
        var $results = $(this).showContentLoading('md').messageRemove();
        var url = $results.data('url');
        jay.ajaxPostHtml({
            url: url,
            data: {
                t: data.searchType,
                q: data.search
            }
        }).done(function(html) {
            var $html = $(html);
            var hData = $html.data();
            $.triggerDataEvent({
                hideJumbotron: hData.empty !== true,
                changeCount: hData.empty === true ? 0 : hData.count
            });
            $results.hideContentLoading().html($html);
            if (hData.empty === true) {
                $results.message('info', 'No search results.  Change your query and try again.');
            }
        }).fail(function() {
            $.triggerDataEvent('hideJumbotron', false);
            $results.hideContentLoading().empty().message('error', jay.failMessage);
        });
    });

$.onAction('changeSearchType',
    function(e, data) {
        $.triggerDataEvent({
            clearResults: true,
            changeSearchPlaceHolder: data,
            changeSearchType: data.text,
            changeCount: 0
        });
    });
$.onAction('delete',
    function (e, data) {
        var $pill = $(this).closest('.list-group-item');
        jay.modal({
            title: 'Confirm Delete',
            size: 'small',
            content: $('<div/>').html('<p>Are you sure you really want to delete this spec?</p>'),
            buttons: [
                {
                    label: 'No, Cancel',
                    close: true
                },
                {
                    label: 'Yes, Delete it',
                    cssClass: 'btn-danger',
                    onClick: function(e, content, mw) {
                        mw.showContentLoading();
                        jay.ajax({
                            url: data.url
                        }).done(function() {
                            mw.hideContentLoading().modal('hide');
                            $pill.slideUp('slow',
                                function() {
                                    $(this).remove();
                                });
                        }).error(function(msg) {
                            mw.hideContentLoading();
                            content.message('error', msg);
                        }).fail(function() {
                            mw.hideContentLoading();
                            content.message('error', msg);
                        });
                    }
                }
            ]
        });
    });

$.onAction('search',
    function (e, data) {
        var values = $(this).getInputValues();
        var sdata = $.extend({}, values, data);
        Lockr.set(data.key, sdata);
        if (sdata.search && sdata.search.length || sdata.searchType === 'Mine') {
            $.triggerDataEvent('search', sdata);
        } else{
            $.triggerDataEvent('hideJumbotron', false);
        }
    });
$.onAction('clone',
    function (e, data) {
        jay.modal({
            title: 'Confirm Clone',
            content:$('<p>This action will copy all attributes and justifications from this specification into a new one.  Do you want to continue or cancel?</p>'),
            size: 'small',
            buttons: [
                {
                    label: 'Cancel',
                    close: true
                }, {
                    label: 'Continue Cloning',
                    onClick: function (e, content, mw) {
                        mw.showContentLoading();
                        jay.ajax({
                            url: data.url
                        }).done(function (json) {
                            window.location.href = json.url;
                        }).error(function (msg) {
                            mw.hideContentLoading();
                            content.message('error', msg);
                        }).fail(function () {
                            mw.hideContentLoading();
                            content.message('error', jay.failMessage);
                        });
                    }
                }
            ]
        });
    });
$.onAction('wireUpSearch',
    function() {  
        var $t = $(this).on('focus',
            function() {
                $(this)
                    .one('blur',
                        function() {
                            $(this).off('keypress');
                        })
                    .on('keypress',
                        function(e) {
                            if (e.which === 13) {
                                $(this).blur();
                            }
                        });
            });
        var data = $t.data();
        var last = Lockr.get(data.key);
        if (last && last.searchType) {
            $('button.action-change-search-type[data-type="' + last.searchType + '"]').doAction('changeSearchType').wait(200,
                function() {
                    if (last.search) {
                        $t.val(last.search).doAction('search');
                    } else if (last.searchType === 'Mine') {
                        $t.doAction('search');
                    } else {
                        $t.trigger('focus');
                    }
                });
        } else {
            $t.trigger('focus');
        }        
    });