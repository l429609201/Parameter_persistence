define(['loading', 'emby-input', 'emby-button', 'emby-checkbox', 'dialogHelper', 'dom'], function (loading, embyInput, embyButton, embyCheckbox, dialogHelper, dom) {
    'use strict';

    var pluginId = '8F6D8C9E-4B2A-4F3E-9D1C-7A8B9C0D1E2F';
    var currentPage;
    var parameters = [];

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
                require(['toast'], function (toast) {
                    toast('保存配置失败，请重试。');
                });
            });
        });

        return false;
    }

    function queryParameters(namespace, key) {
        loading.show();

        var requestData = {};
        if (namespace) requestData.Namespace = namespace;
        if (key) requestData.Key = key;

        ApiClient.fetch({
            type: 'POST',
            url: ApiClient.getUrl('/ParameterPersistence/Query'),
            data: JSON.stringify(requestData),
            contentType: 'application/json',
            dataType: 'json'
        }).then(function (response) {
            parameters = response.Parameters || [];
            renderParameterList();
            loading.hide();
        }).catch(function (error) {
            loading.hide();
            require(['toast'], function (toast) {
                toast('查询参数失败：' + (error.message || '未知错误'));
            });
        });
    }

    function renderParameterList() {
        var container = currentPage.querySelector('#parameterList');
        container.innerHTML = '';
        currentPage.querySelector('#parameterCount').textContent = parameters.length;

        if (parameters.length === 0) {
            container.innerHTML = '<div class="fieldDescription" style="padding: 2em; text-align: center; color: #999;">暂无参数数据，点击"添加参数"按钮创建新参数</div>';
            return;
        }

        var html = '';
        parameters.forEach(function (param) {
            var valueText = param.Value || '';
            if (valueText.length > 150) {
                valueText = valueText.substring(0, 150) + '...';
            }

            var timeText = '';
            if (param.CreatedAt) {
                timeText = '创建: ' + new Date(param.CreatedAt).toLocaleString('zh-CN');
            }
            if (param.UpdatedAt) {
                timeText += (timeText ? ' | ' : '') + '更新: ' + new Date(param.UpdatedAt).toLocaleString('zh-CN');
            }

            html += '<div class="listItem listItem-border" style="padding: 1em; margin-bottom: 0.5em;">';
            html += '<div style="display: flex; justify-content: space-between; align-items: start;">';
            html += '<div style="flex: 1;">';
            html += '<div style="font-weight: bold; color: #00a4dc; margin-bottom: 0.3em;">命名空间: ' + param.Namespace + '</div>';
            html += '<div style="font-size: 1.1em; margin-bottom: 0.5em;">键名: ' + param.Key + '</div>';
            html += '<div style="color: #999; font-family: monospace; white-space: pre-wrap; word-break: break-all; margin-bottom: 0.5em;">值: ' + valueText + '</div>';
            if (timeText) {
                html += '<div style="color: #666; font-size: 0.9em;">' + timeText + '</div>';
            }
            html += '</div>';
            html += '<div style="margin-left: 1em; display: flex; gap: 0.5em;">';
            html += '<button type="button" is="paper-icon-button-light" class="parameterEditBtn" data-namespace="' + param.Namespace + '" data-key="' + param.Key + '" title="编辑">';
            html += '<i class="md-icon">edit</i>';
            html += '</button>';
            html += '<button type="button" is="paper-icon-button-light" class="parameterDeleteBtn" data-namespace="' + param.Namespace + '" data-key="' + param.Key + '" title="删除">';
            html += '<i class="md-icon">delete</i>';
            html += '</button>';
            html += '</div>';
            html += '</div>';
            html += '</div>';
        });

        container.innerHTML = html;

        // 绑定编辑按钮事件
        container.querySelectorAll('.parameterEditBtn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var namespace = this.getAttribute('data-namespace');
                var key = this.getAttribute('data-key');
                var param = parameters.find(function (p) {
                    return p.Namespace === namespace && p.Key === key;
                });
                if (param) {
                    showParameterDialog(param);
                }
            });
        });

        // 绑定删除按钮事件
        container.querySelectorAll('.parameterDeleteBtn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var namespace = this.getAttribute('data-namespace');
                var key = this.getAttribute('data-key');
                var param = parameters.find(function (p) {
                    return p.Namespace === namespace && p.Key === key;
                });
                if (param) {
                    deleteParameter(param);
                }
            });
        });
    }

    function showParameterDialog(param) {
        var isEdit = !!param;
        var dialogHtml = '';

        dialogHtml += '<div class="formDialogContent smoothScrollY" style="padding: 2em;">';
        dialogHtml += '<div class="dialogContentInner dialog-content-centered">';
        dialogHtml += '<form style="margin: auto;">';

        dialogHtml += '<div class="inputContainer">';
        dialogHtml += '<label class="inputLabel inputLabelUnfocused" for="paramNamespace">命名空间 *</label>';
        dialogHtml += '<input id="paramNamespace" type="text" is="emby-input" required ' + (isEdit ? 'readonly' : '') + ' value="' + (param ? param.Namespace : '') + '" />';
        dialogHtml += '<div class="fieldDescription">参数的命名空间，用于分组管理</div>';
        dialogHtml += '</div>';

        dialogHtml += '<div class="inputContainer">';
        dialogHtml += '<label class="inputLabel inputLabelUnfocused" for="paramKey">键名 *</label>';
        dialogHtml += '<input id="paramKey" type="text" is="emby-input" required ' + (isEdit ? 'readonly' : '') + ' value="' + (param ? param.Key : '') + '" />';
        dialogHtml += '<div class="fieldDescription">参数的唯一标识符</div>';
        dialogHtml += '</div>';

        dialogHtml += '<div class="inputContainer">';
        dialogHtml += '<label class="inputLabel inputLabelUnfocused" for="paramValue">值 *</label>';
        dialogHtml += '<textarea id="paramValue" is="emby-textarea" rows="8" required>' + (param ? param.Value : '') + '</textarea>';
        dialogHtml += '<div class="fieldDescription">参数的值，支持任意文本内容</div>';
        dialogHtml += '</div>';

        dialogHtml += '<div class="formDialogFooter">';
        dialogHtml += '<button is="emby-button" type="submit" class="raised button-submit block formDialogFooterItem emby-button">';
        dialogHtml += '<span>' + (isEdit ? '更新' : '创建') + '</span>';
        dialogHtml += '</button>';
        dialogHtml += '<button is="emby-button" type="button" class="raised formDialogFooterItem block btnCancel emby-button">';
        dialogHtml += '<span>取消</span>';
        dialogHtml += '</button>';
        dialogHtml += '</div>';

        dialogHtml += '</form>';
        dialogHtml += '</div>';
        dialogHtml += '</div>';

        var dlg = dialogHelper.createDialog({
            size: 'small',
            removeOnClose: true,
            scrollY: false
        });

        dlg.classList.add('formDialog');
        dlg.innerHTML = dialogHtml;

        dlg.querySelector('.btnCancel').addEventListener('click', function () {
            dialogHelper.close(dlg);
        });

        dlg.querySelector('form').addEventListener('submit', function (e) {
            e.preventDefault();

            var namespace = dlg.querySelector('#paramNamespace').value.trim();
            var key = dlg.querySelector('#paramKey').value.trim();
            var value = dlg.querySelector('#paramValue').value;

            if (!namespace || !key || !value) {
                require(['toast'], function (toast) {
                    toast('请填写所有必填字段');
                });
                return false;
            }

            saveParameter(namespace, key, value, isEdit);
            dialogHelper.close(dlg);
            return false;
        });

        dialogHelper.open(dlg);
    }

    function saveParameter(namespace, key, value, isEdit) {
        loading.show();

        var url = isEdit ? '/ParameterPersistence/Update' : '/ParameterPersistence/Create';
        var requestData = {
            Namespace: namespace,
            Key: key,
            Value: value
        };

        ApiClient.fetch({
            type: 'POST',
            url: ApiClient.getUrl(url),
            data: JSON.stringify(requestData),
            contentType: 'application/json',
            dataType: 'json'
        }).then(function () {
            loading.hide();
            require(['toast'], function (toast) {
                toast(isEdit ? '参数更新成功' : '参数创建成功');
            });
            var searchNamespace = currentPage.querySelector('#searchNamespace').value.trim();
            var searchKey = currentPage.querySelector('#searchKey').value.trim();
            queryParameters(searchNamespace, searchKey);
        }).catch(function (error) {
            loading.hide();
            require(['toast'], function (toast) {
                toast('保存参数失败：' + (error.message || '未知错误'));
            });
        });
    }

    function deleteParameter(param) {
        require(['confirm'], function (confirm) {
            confirm('确定要删除参数 "' + param.Namespace + '.' + param.Key + '" 吗？', '确认删除').then(function () {
                loading.show();

                var requestData = {
                    Namespace: param.Namespace,
                    Key: param.Key
                };

                ApiClient.fetch({
                    type: 'POST',
                    url: ApiClient.getUrl('/ParameterPersistence/Delete'),
                    data: JSON.stringify(requestData),
                    contentType: 'application/json',
                    dataType: 'json'
                }).then(function () {
                    loading.hide();
                    require(['toast'], function (toast) {
                        toast('参数删除成功');
                    });
                    var searchNamespace = currentPage.querySelector('#searchNamespace').value.trim();
                    var searchKey = currentPage.querySelector('#searchKey').value.trim();
                    queryParameters(searchNamespace, searchKey);
                }).catch(function (error) {
                    loading.hide();
                    require(['toast'], function (toast) {
                        toast('删除参数失败：' + (error.message || '未知错误'));
                    });
                });
            });
        });
    }

    return function (view, params) {
        currentPage = view;

        view.querySelector('#ParameterConfigForm').addEventListener('submit', onSubmit);

        view.querySelector('#searchBtn').addEventListener('click', function () {
            var namespace = view.querySelector('#searchNamespace').value.trim();
            var key = view.querySelector('#searchKey').value.trim();
            queryParameters(namespace, key);
        });

        view.querySelector('#clearSearchBtn').addEventListener('click', function () {
            view.querySelector('#searchNamespace').value = '';
            view.querySelector('#searchKey').value = '';
            queryParameters('', '');
        });

        view.querySelector('#addParameterBtn').addEventListener('click', function () {
            showParameterDialog(null);
        });

        view.addEventListener('viewshow', function () {
            loading.show();

            ApiClient.getPluginConfiguration(pluginId).then(function (config) {
                loadConfig(view, config);

                // 获取插件信息并显示版本号
                ApiClient.getInstalledPlugins().then(function (plugins) {
                    var plugin = plugins.find(function (p) { return p.Id === pluginId; });
                    if (plugin) {
                        view.querySelector('#pluginVersion').textContent = 'v' + plugin.Version;
                    }
                });

                loading.hide();
                queryParameters('', '');
            }, function () {
                loading.hide();
                require(['toast'], function (toast) {
                    toast('加载配置失败，请重试。');
                });
            });
        });
    };
});

