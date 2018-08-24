$.onDataEvent('clearResults',
    '#results',
    function (b) {
        if (b === true) {
            $(this).html($('<h3>').addInlineLoading('loading...'));
        } else {
            $(this).html($('<div/>').addClass('jumbotron').html('<h2>No Errors Found</h2>')
                .append('<p>Click the <i class="fa fa-hourglass-half"></i> button to refresh the data from the Chef Automate Server.</p>'));
        }
    });

$.onDataEvent('showResults',
    '#results',
    function (h) {
        $(this).html(h);
    });
$.onDataEvent('specName',
    'span.spec-name',
    function (h) {
        $(this).html(h);
    });
$.onAction('popup',
    function(e, data) {
        jay.modal({
            title: data.title,
            size: 'xl',
            partialUrl: data.url,
            partialData: {
                name: data.name,
                code: data.code
            }
        });
    });
$.onAction('add',
    function (e, data) {
        var $btn = $(this);
        var $div = $btn.closest('div').messageRemove().showContentLoading('sm');
        jay.ajax({
            url: data.url,
            data: {
                name: data.name,
                description: 'added by fix-it-for-me',
                specId: data.specId
            }
        }).done(function () {
            $div.hideContentLoading().message('success', 'Fixed it. Ensure you add the justification manually.');
            $btn.remove();
        }).error(function(err) {
            $div.hideContentLoading().message('error', err);
        }).fail(function() {
            $div.hideContentLoading().message('error', jay.failMessage);
        });
    });
$.onAction('remove',
    function (e, data) {
        var $btn = $(this);
        var $div = $btn.closest('div').messageRemove().showContentLoading('sm');
        jay.ajax({
            url: data.url
        }).done(function () {
            $div.hideContentLoading().message('success', 'Fixed it.');
            $btn.remove();
        }).error(function (err) {
            $div.hideContentLoading().message('error', err);
        }).fail(function () {
            $div.hideContentLoading().message('error', jay.failMessage);
        });
    });
$.onAction('results',
    function () {
        var $t = $(this);
        var $form = $t.closest('div[role="form"]').showContentLoading();
        var value = '';
        if ($t.is('#environment')) {
            value = $t.val();
        } else {
            value = $form.getInputValues().environment;
        }
        var optData = $('#environment', $form).find('option[value="' + value + '"]').data();
        $.triggerDataEvent({
            clearResults: true
        });
        jay.ajaxPostHtml({
            url: optData.reviewUrl
        }).done(function (html) {
            var $html = $(html);
            var hData = $html.data();
            $form.hideContentLoading().messageRemove();
            if (hData.success) {
                $.triggerDataEvent({
                    clearResults: false,
                    specName: hData.name
                });
            } else {
                $.triggerDataEvent({
                    showResults: $html,
                    specName: hData.name
                });
            }
        }).fail(function () {
            $.triggerDataEvent('clearResults', false);
            $form.hideContentLoading().message('error', jay.failMessage);
        });
    });
$.onAction('retrieve',
    function () {
        var $t = $(this);
        var $form = $t.closest('div[role="form"]').message('Gathering aggregate data from Chef.').showContentLoading();
        var value = $form.getInputValues().environment;
        var optData = $('#environment', $form).find('option[value="' + value + '"]').data();

        $.triggerDataEvent('clearResults', true);
        jay.ajax({
            url: optData.gatherUrl
        }).done(function () {
            $form.hideContentLoading().message('loading', 'Gathering Nodes...').showContentLoading();

            jay.ajax({
                url: optData.nodesUrl
            }).done(function (json) {
                if (json && json.urls && json.urls.length) {
                    var ln = json.urls.length;
                    $form.hideContentLoading().message('loading', 'Gathering Node Reports... 0 of ' + ln).showContentLoading();
                    var count = 0;
                    var interval = window.setInterval(function () {
                        if (count === ln) {
                            window.clearInterval(interval);
                            $form.hideContentLoading().message('loading', 'Aggregating Data...').showContentLoading();
                            $t.doAction('results');
                        } else {
                            $form.hideContentLoading().message('loading', 'Gathering Node Reports... '+ count + ' of ' + ln).showContentLoading();
                        }
                    },
                        1000);
                    for (var i = 0; i < ln; i++) {
                        jay.ajax({
                            url: json.urls[i]
                        }).done(function () {
                            count += 1;
                        }).error(function () {
                            count += 1;
                        }).fail(function () {
                            count += 1;
                        });
                    }
                } else {
                    $.triggerDataEvent('clearResults', false);
                    $form.hideContentLoading().message('info', 'No Nodes returned.');
                }
            }).error(function (message) {
                $.triggerDataEvent('clearResults', false);
                $form.hideContentLoading().message('error', message);
            }).fail(function () {
                $.triggerDataEvent('clearResults', false);
                $form.hideContentLoading().message('error', jay.failMessage);
            });

        }).error(function (message) {
            $.triggerDataEvent('clearResults', false);
            $form.hideContentLoading().message('error', message);
        }).fail(function () {
            $.triggerDataEvent('clearResults', false);
            $form.hideContentLoading().message('error', jay.failMessage);
        });
    });
