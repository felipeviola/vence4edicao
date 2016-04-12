var global = {

    init: function () {
        this.config();
        eventos.onClick();
        global.formValidation();
    },

    params: {
        $Chave: null,
        $btnPesquisa: null,
        $btnImprimir: null,
        $frmPesquisa: null
    },

    config: function () {
        this.params.$Chave = $("#Chave");
        this.params.$btnPesquisa = $("#btnPesquisa");
        this.params.$btnImprimir = $("#btnImprimir");
        this.params.$frmPesquisa = $("#frmPesquisa");
    },

    formValidation: function () {
        $("#frmPesquisa").validate({
            rules: {
                Chave: { required: true }
            },
            messages: {
                Chave: { required: 'Obrigatório!' }
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

    onClick: function () {

        global.params.$btnPesquisa.on('click', function () {
            $('#gvCargaHoraria').html('');
           
            if (global.params.$frmPesquisa.valid() === true) {

                eventos.pesquisarCargaHoraria();
            }
        });

        global.params.$btnImprimir.on('click', function () {
            if (global.params.$frmPesquisa.valid() === true) {
                eventos.gerarArquivoExcel();
            }
        });
    },

    gerarRelatorio: function (e) {
        var chave = global.params.$Chave.val();
        var url = window.location.href.split('?', 1) + "/GerarRelatorio?chave=" + chave;
        window.location = url;
    },

    gerarArquivoExcel: function () {
        var chave = global.params.$Chave.val();
        var url = window.location.href.split('?', 1) + "/GerarArquivosExcel?chave=" + chave;
        window.location = url;
    },

    pesquisarCargaHoraria: function () {

        var chave = global.params.$Chave.val();

        global.showAjaxLoader();

        $.ajax({
            type: 'GET',
            async: true,
            dataType: 'html',
            url: '../CargaHoraria/PesquisarCargaHoraria',
            data: { chave: chave },
            success: function (data) {
             
                if (data != undefined && data.length > 0) {

                    $('#gvCargaHoraria').html(data);
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