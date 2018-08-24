$.onDataEvent('showHideCoreOs',
    'div#coreoswrapper',
    function (value) {
        $(this)[value.workflow === 'windows' ? 'slideDown' : 'slideUp']('fast');
    });
$.onDataEvent('specNameChange',
    'h3.build-spec-name',
    function (value) {
        var $t = $(this);
        var data = $t.data();
        if (value === '')
            value = $('<em/>').html(data.default);
        $t.html(value);
    });

$.onDataEvent('scrollTop',
    'div.body-content',
    function (duration) {
        $('html,body').animate({
                scrollTop: $(this).offset().top - 100
            },
            parseInt(duration,10) || 700);
    });

$.onDataEvent('stepsChange',
    'span.steps',
    function (steps) {        
        $(this).html(steps);
    });

$.onDataEvent('stepChange',
    'span.step',
    function (step) {
        $(this).html(step);
    });

$.onDataEvent('showHideNextButton',
    'button.action-next',
    function (data) {
        $(this)[data.index === (data.steps - 1) ? 'hide' : 'show']();
    });

$.onDataEvent('showHidePreviousButton',
    'button.action-previous',
    function (data) {
        $(this)[(data.index > 0) ? 'show' : 'hide']();
    });

$.onDataEvent('showHideDoneButton',
    'button.action-done',
    function (data) {
        $(this)[data.index === (data.steps - 1) ? 'show' : 'hide']();
    });

$.onDataEvent('wizardBeforeStepFunctionChange',
    '#area',
    function (d) {
        var $t = $(this);
        //retrieve
        var data = $t.data('widget');
        //tweak
        if (d && d.fn && $.isFunction(d.fn)) {
            data.beforeStep = d.fn;
        }
        //save back
        $t.data('widget', data);
    });


$.onDataEvent('wizardPlatformChange',
    '#area',
    function (wf) {
        if (wf){ 
            var $t = $(this);
            //retrieve
            var data = $t.data('widget');
            //tweak
            data.workflow = wf;
            data.steps = data.order[wf].length;
            //save back
            $t.data('widget', data);
            //trigger events to wizard buttons
            $.triggerDataEvent({
                stepsChange: data.steps,
                showHidePreviousButton: data,
                showHideNextButton: data,
                showHideDoneButton: data,
                showHideCoreOs: data
            });
        }
    });
$.onDataEvent('wizardHideButtonArea',
    'div.button-area',
    function(bool) {
        $(this)[bool === true ? 'slideUp' : 'slideDown']();
    });
$.onDataEvent('wizardDone',
    '#area',
    function(url) {
        var $t = $(this).showContentLoading();
        var data = $t.data('widget');
        jay.ajaxPostHtml({
            url: url,
            data: {
                id: data.id
            }
        }).done(function (html) {
            var $html = $(html);
            //hide loading stuff
            $t.hideContentLoading().html($html);
        }).fail(function () {
            //hide loading stuff and display error
            $t.hideContentLoading().message('error', jay.failMessage);
        });
    });
$.onDataEvent('wizardStepChange',
    '#area',
    function (inc) {
        //show loading stuff
        var $t = $(this).showContentLoading();
        var widget = $t.data('widget');
        //create a function to do the move 
        var doMove = function (data) {
            var steps = data.steps;
            //default to whats there
            inc = inc ? parseInt(inc, 10) || 0 : 0;
            var index = data.index + inc;
            //trap for negative index
            index = index < 0 ? 0 : index;
            //trap for greater than max
            index = index > steps - 1 ? steps - 1 : index;
            //tweak
            data.index = index;
            data.beforeStep = null;
            var fromClone = data.fromClone === true && data.index === 0;
            data.fromClone = false;
            //save back
            var $this = $(this).data(data);
            //load content
            jay.ajaxPostHtml({
                url: data.order[data.workflow][index],
                data: {
                    specId: data.id
                }
            }).done(function(html) {
                var $html = $(html);
                //hide loading stuff
                $.hideAllContentLoading();
                $this.html($html);
                if (fromClone) {
                    $html.find(':input[name="name"]').val('');
                }
                $.triggerDataEvent({
                    stepChange: data.index + 1,
                    showHidePreviousButton: data,
                    showHideNextButton: data,
                    showHideDoneButton: data,
                    scrollTop: 20
                });
            }).fail(function() {
                //hide loading stuff and display error
                $this.hideContentLoading().message('error', jay.failMessage);
            });
        };

        //run logic first if applicable
        if (inc !== 0 && widget.beforeStep && $.isFunction(widget.beforeStep)) {
            widget.beforeStep.apply($t,
                [
                    widget,
                    function() {
                        doMove.apply($t, [widget]);
                    }
                ]);
        } else {
            doMove.apply($t, [widget]);
        }
    });
$.onAction('next',
    function () {
        $.triggerDataEvent('wizardStepChange', 1);
    });

$.onAction('previous',
    function () {
        $.triggerDataEvent('wizardStepChange', -1);
    });

$.onAction('done',
    function (evt, data) {
        $.triggerDataEvent({
            wizardDone: data.url,
            wizardHideButtonArea: true
        });
    });

$.onAction('initializeWizard',
    function (e, data) {        
        $(this)
            .data('widget',
                {
                    id: data.id,
                    index: 0,
                    workflow: 'linux',
                    beforeStep: null,
                    steps: data.linuxOrder.length,
                    order: {
                        linux: data.linuxOrder,
                        unix: data.unixOrder,
                        windowsserver: data.windowsServerOrder,
                        windowsclient: data.windowsClientOrder,
                        other: data.otherOrder,
                        appliance: data.applianceOrder
                    },
                    fromClone: data.fromClone
                });
        $.triggerDataEvent('wizardStepChange');
    });