$.onDataEvent('osNamePlaceHolderChange',
    ':input:text[name="osname"]',
    function (value) {
        $(this).attr('placeholder', 'OS Name (ex. ' + value + ')');
    });

$.onAction('platformChanged',
    function (evt, data) {
        var $t = $(this);
        var val = $t.val();
        data = $.extend({}, data, $('option[value="' + val + '"]', $t).data());
        $.triggerDataEvent({
            osNamePlaceHolderChange: data.text,
            wizardPlatformChange: val
        });
    });

$.onAction('osSpecNameChanged',
    function () {
        $.triggerDataEvent('specNameChange', $(this).val());
    });

$.onAction('initializeInfoWidget',
    function (e, data) {


        $.triggerDataEvent('wizardBeforeStepFunctionChange',
            {
                fn:
                    function(widget, success) {
                        var $t = $(this).clearJsonValidation();
                        jay.ajax({
                            url: data.url,
                            data: $t.getInputValues()
                        }).done(function(json) {
                            widget.id = json.id;
                            success.apply($t);
                        }).error(function(error) {
                            $t.hideContentLoading().applyJsonValidation(error);
                        }).fail(function() {
                            $t.hideContentLoading().message('error', jay.failMessage);
                        });
                    }
            });

        $('select.action-platform-changed', this).doAction('platformChanged');
        $(':input:text.action-os-spec-name-changed', this).doAction('osSpecNameChanged');
    });

