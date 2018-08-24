$.onAction('appSpecNameChanged',
    function () {
        $.triggerDataEvent('specNameChange', $(this).val());
    });

$.onDataEvent('osSpecLink', 
    'button.action-os-spec-link',
    function (d) {
        if (d && d.i){ 
            $(this).data('id', d.i);
        } else{
            $(this).data('id', null);
        }
    });

$.onAction('osSpecLink',
    function (e, data) {
        if (data.id){
            location.href = data.url + '/' + data.id;
        } else{
            jay.alert('Think first, then ACT', 'First search for an OS Spec.');
        }
    });

$.onAction('initializeInfoWidget',
    function(e, data) {
        $.triggerDataEvent('wizardBeforeStepFunctionChange',
            {
                fn:
                    function(widget, success) {
                        var $t = $(this);
                        $t.clearJsonValidation();
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
        $.triggerDataEvent('wizardPlatformChange', data.platform);
        $('select.action-os-spec-changed', this).doAction('osSpecChanged');
        $(':input:text.action-app-spec-name-changed', this).doAction('appSpecNameChanged');
    });

$.onAction('afterTypeAhead',
    function (e, data) {
        $.triggerDataEvent({
            wizardPlatformChange: data.p,
            osSpecLink: data
        });
    });