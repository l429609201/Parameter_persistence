define(['loading', 'emby-input', 'emby-button', 'emby-checkbox'], function (loading) {
    'use strict';

    var pluginId = '8F6D8C9E-4B2A-4F3E-9D1C-7A8B9C0D1E2F';

    function loadConfig(page, config) {
        page.querySelector('#EnableLogging').checked = config.EnableLogging || false;
        page.querySelector('#MaxParameterCount').value = config.MaxParameterCount || 10000;
    }

    function onSubmit(e) {
        e.preventDefault();
        loading.show();

        var form = this;
        var page = form.closest('.page');

        ApiClient.getPluginConfiguration(pluginId).then(function (config) {
            config.EnableLogging = page.querySelector('#EnableLogging').checked;
            config.MaxParameterCount = parseInt(page.querySelector('#MaxParameterCount').value) || 10000;

            ApiClient.updatePluginConfiguration(pluginId, config).then(function (result) {
                loading.hide();
                Dashboard.processPluginConfigurationUpdateResult(result);
            }, function () {
                loading.hide();
                Dashboard.alert('保存配置失败，请重试。');
            });
        });

        return false;
    }

    return function (view, params) {
        view.querySelector('#ParameterConfigForm').addEventListener('submit', onSubmit);

        view.addEventListener('viewshow', function () {
            loading.show();

            ApiClient.getPluginConfiguration(pluginId).then(function (config) {
                loadConfig(view, config);
                loading.hide();
            }, function () {
                loading.hide();
                Dashboard.alert('加载配置失败，请重试。');
            });
        });
    };
});

