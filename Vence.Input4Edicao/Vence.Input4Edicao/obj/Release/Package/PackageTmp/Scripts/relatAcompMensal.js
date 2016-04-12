var global = {

    init: function () {
        this.config();
        eventos.onClick();
        eventos.onBlur();
        global.formValidation();
    },

    params: {
        $NumeroAES: null,
        $ItemAES: null,
        $btnPesquisa: null,
        $frmPesquisa: null
    },

    config: function () {
        this.params.$NumeroAES = $("#NumeroAES");
        this.params.$btnPesquisa = $("#btnPesquisa");
        this.params.$ItemAES = $("#ItemAES");
        this.params.$frmPesquisa = $("#frmPesquisa");
    },

    formValidation: function () {
        $("#frmPesquisa").validate({
            rules: {
                NumeroAES: { required: true },
                ItemAES: { required: true }
            },
            messages: {
                NumeroAES: { required: 'Obrigatório!' },
                ItemAES: { required: 'Obrigatório!' }
            }
        });
    },

    mensageSucess: function (message) {
        alert(message);
    },

    mensageError: function (message) {
        alert(message);
    },

    showAjaxLoader: function () {
        $("#ajaxLoading").removeClass("imgHidde");
        $("#ajaxLoading").addClass("imgVisible");
    },

    hideAjaxLoader: function () {
        $("#ajaxLoading").removeClass("imgVisible");
        $("#ajaxLoading").addClass("imgHidde");
    }
}

var eventos = {

    onBlur: function () {

        global.params.$NumeroAES.on('blur', function () {
            eventos.pesquisarItemAESPorNumeroAES($(this).val());
        });
    },

    onClick: function () {

        global.params.$btnPesquisa.on('click', function () {
            $('#gvAcompMensal').html('');
            if (global.params.$frmPesquisa.valid() === true) {
                eventos.pesquisar();
            }
        });
    },

    pesquisarItemAESPorNumeroAES: function (numeroAES) {
        $.ajax({
            type: 'GET',
            async: true,
            dataType: 'json',
            url: '../RelatAcompMensal/PesquisarItemAESPorNumeroAES',
            data: { numeroAES: numeroAES },
            success: function (data) {
                if (data != undefined && data.length > 0) {
                    global.params.$ItemAES.empty().append($("<option></option>").attr("value", "0").text("Selecione..."));
                    for (var i = 0; i < data.length; i++) {
                        global.params.$ItemAES.append($("<option></option>").attr("value", data[i].ItemAES).text(data[i].ItemAES));
                    }
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $(document).ajaxStop(function () {
                    global.mensageError('Ocorreu um erro durante o processo.');
                });
            }
        });
    },

    pesquisar: function (e) {

        global.showAjaxLoader();

        var numeroAES = global.params.$NumeroAES.val();
        var itemAES = global.params.$ItemAES.val();

        $.ajax({
            type: 'GET',
            async: true,
            dataType: 'html',
            url: '../RelatAcompMensal/Pesquisar',
            data: { numeroAES: numeroAES, itemAES: itemAES },
            success: function (data) {
                $('#gvAcompMensal').html(data);
                global.hideAjaxLoader();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $(document).ajaxStop(function () {
                    global.mensageError('Ocorreu um erro durante o processo.');
                    global.hideAjaxLoader();
                });                
            }
        });
    }
}

$(document).ready(function () {
    global.init();
});