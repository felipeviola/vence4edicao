var global = {

    init: function () {        
        this.config();
        this.limparCampos();
        eventos.onClick();
        eventos.onBlur();
        global.formValidation();
    },

    params: {
        $Chave: null,
        $NumeroAES: null,
        $ItemAES: null,
        $btnPesquisa: null,
        $btnImprimir: null,
        $frmPesquisa: null
    },

    config: function () {
        this.params.$Chave = $("#Chave");
        this.params.$NumeroAES = $('#NumeroAES');
        this.params.$ItemAES = $('#ItemAES');
        this.params.$btnPesquisa = $("#btnPesquisa");
        this.params.$btnImprimir = $("#btnImprimir");
        this.params.$frmPesquisa = $("#frmPesquisa");
    },

    formValidation: function () {
        $("#frmPesquisa").validate({
            rules: {
                Chave: { required: true },
                NumeroAES: { required: true },
                ItemAES: { required: true }
            },
            messages: {
                Chave: { required: "Obrigatório!" },
                NumeroAES: { required: "Obrigatório!" },
                ItemAES: { required: "Obrigatório!" }
            }
        });
    },

    mensageSucess: function (message) {
        alert(message);
    },

    mensageError: function (message) {
        alert(message);
    },

    mensageWarning: function (message) {
        alert(message);
    },

    limparCampos: function () {
        global.params.$Chave.val('');
        global.params.$NumeroAES.val('');        
        this.limparItemAES();
    },

    limparItemAES: function () {
        global.params.$ItemAES.empty().append($("<option></option>").attr("value", "").text("Selecione..."));
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

        global.params.$Chave.on('blur', function () {
            eventos.pesquisarNumeroAESPorChave($(this).val());
        });
    },

    onClick: function () {

        global.params.$btnPesquisa.on('click', function () {
            $('#gvEstagio').html('');
            if (global.params.$frmPesquisa.valid() === true) {
                eventos.pesquisarEstagios();
            }
        });

        global.params.$btnImprimir.on('click', function () {
            if (global.params.$frmPesquisa.valid() === true) {
                eventos.gerarRelatorio();
            }
        });
    },

    pesquisarNumeroAESPorChave: function (chave) {
        if (chave == '')
            return;        

        $.ajax({
            type: 'GET',
            async: true,
            dataType: 'json',
            url: '../Estagio/PesquisarNumeroAESPorChave',
            data: { chave: chave },
            success: function (data) {
                if (data != undefined && data.length > 0) {
                    global.params.$NumeroAES.val(data[0].NumeroAES);                    
                    global.limparItemAES();
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

    gerarArquivoExcel: function () {
        var chave = global.params.$Chave.val();
        var url = window.location.href.split('?', 1) + "/GerarArquivosExcel?chave=" + chave;
        window.location = url;
    },

    gerarRelatorio: function (e) {

        var numeroAES = global.params.$NumeroAES.val();
        var itemAES = global.params.$ItemAES.val();
        var url = window.location.href.split('?', 1) + "/GerarRelatorio?numeroAES=" + numeroAES + "&itemAES=" + itemAES;
        window.location = url;
    },

    pesquisarEstagios: function (e) {

        global.showAjaxLoader();

        var numeroAES = global.params.$NumeroAES.val();
        var itemAES = global.params.$ItemAES.val();

        $.ajax({
            type: 'GET',
            async: true,
            dataType: 'html',
            url: '../Estagio/PesquisarEstagios',
            data: { numeroAES: numeroAES, itemAES: itemAES },
            success: function (data) {
                if (data != undefined && data.length > 0) {
                    $('#gvEstagio').html(data);
                    if ($(data).find('#labelgrid').length == 0) {
                        document.getElementById('btnImprimir').style.display = 'block';
                    } else {
                        document.getElementById('btnImprimir').style.display = 'none';
                    }
                } else {
                    document.getElementById('btnImprimir').style.display = 'none';
                }
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