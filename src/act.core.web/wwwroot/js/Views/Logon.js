$.onAction('logOn',
    function (evt, data) {
        var $t = $(this);
        var $form = $t.closest('form').showContentLoading().messageRemove();

        jay.ajax({
            url: data.url,
            data: $form.getInputValues()
        }).done(function(json) { //from JsonEnvelope.Success()
            $t.hide();
            $form.hideContentLoading().message('loading', 'Redirecting...').showContentLoading();
            self.location.replace(json.url);
        }).error(function(message) { //from JsonEnvelope.Error()               
            $form.hideContentLoading().message('error', message);
        }).fail(function() { //from exception or timeout
            $form.hideContentLoading().message('error', jay.failMessage);
        });
    });

$.onAction('logonFormVisible',
    function (evt, data) {
        if (data.isLoggedOn === true) {
            $(this).message('info', data.message);
        }
    });