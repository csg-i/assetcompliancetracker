$.onAction('loadScoreCard',
    function (e, data) {
        if (data.type) {
            Lockr.prefix = data.type;
        }
        var $t = $(this).on('reloadscorecard',
            function(e2, url, hash, name) {
                var $t = $(this).html($('<h3/>').addInlineLoading('loading...'));
                jay.ajaxPostHtml({
                    url: url
                }).done(function(html) {
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
                }).fail(function() {
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
    

$.onAction('wireUpTypeAheadSearch',
    function (e, data) {
        var $t = $(this)
            .on('typeaheadfailure',
                function () {
                    $t.closest('div[role="form"]').message('error', jay.failMessage);
                })
            .on('typeaheadsuccess',
                function (e1, value, text) {
                    $(data.target).trigger('reloadscorecard', [data.scoreCardUrl + '/' + value, value, text]);
                })            
            .doAction('wireUpTypeAhead');
    });

$.onAction('portPopup',
    function (e, data) {
        jay.modal({
            title: 'Port Report',
            size: 'xl',
            partialUrl: data.url
        });
    });