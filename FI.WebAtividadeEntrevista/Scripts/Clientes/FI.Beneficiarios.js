(function ($, window) {
    var beneficiarios = [];
    var idsExcluidos = []; // Ids já gravados no banco que o usuário excluiu na grid
    var indiceEmEdicao = -1;

    function normalizarCpf(cpf) {
        return (cpf || '').replace(/\D/g, '');
    }

    // Mesma regra de CpfUtils.ValidarCPF (servidor)
    function validarCpf(cpf) {
        var numeros = normalizarCpf(cpf);

        if (numeros.length !== 11 || /^(\d)\1{10}$/.test(numeros))
            return false;

        function calcularDigito(tamanho) {
            var soma = 0;

            for (var i = 0; i < tamanho; i++)
                soma += parseInt(numeros.charAt(i), 10) * (tamanho + 1 - i);

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        return calcularDigito(9) === parseInt(numeros.charAt(9), 10) &&
            calcularDigito(10) === parseInt(numeros.charAt(10), 10);
    }

    function formatarCpfParaExibicao(cpf) {
        var campoTemporario = $('<input type="text" />');

        campoTemporario.mask('000.000.000-00');
        campoTemporario.val(cpf || '').trigger('input');

        return campoTemporario.val();
    }

    function ModalDialog(titulo, texto) {
        var id = 'modalMensagemBeneficiario' +
            Math.random().toString().replace('.', '');

        var modal = $(
            '<div class="modal fade" tabindex="-1" role="dialog" aria-hidden="true">' +
            '<div class="modal-dialog">' +
            '<div class="modal-content">' +
            '<div class="modal-header">' +
            '<button type="button" class="close" data-dismiss="modal" aria-label="Fechar">&times;</button>' +
            '<h4 class="modal-title"></h4>' +
            '</div>' +
            '<div class="modal-body"><p></p></div>' +
            '<div class="modal-footer">' +
            '<button type="button" class="btn btn-default" data-dismiss="modal">Fechar</button>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '</div>'
        );

        modal.attr('id', id);
        modal.find('.modal-title').text(titulo);
        modal.find('.modal-body p').text(texto);
        modal.on('hidden.bs.modal', function () {
            $(this).remove();
        });

        $('body').append(modal);
        modal.modal('show');
    }

    function limparCampos() {
        $('#formBeneficiario')[0].reset();
        $('#BenefIndiceEdicao').val('-1');
        $('#btnIncluirBeneficiario').text('Incluir');
        indiceEmEdicao = -1;
    }

    function renderizar() {
        var tbody = $('#tbodyBeneficiarios');
        tbody.empty();

        if (beneficiarios.length === 0) {
            tbody.append(
                $('<tr>').append(
                    $('<td>')
                        .attr('colspan', 3)
                        .addClass('text-center')
                        .text('Nenhum beneficiário informado.')
                )
            );
            return;
        }

        $.each(beneficiarios, function (index, beneficiario) {
            var linha = $('<tr>');
            var celulaAcoes = $('<td>').addClass('text-center');

            linha.append(
                $('<td>').text(formatarCpfParaExibicao(beneficiario.CPF))
            );
            linha.append(
                $('<td>').text(beneficiario.Nome || '')
            );

            $('<button>')
                .attr('type', 'button')
                .attr('data-index', index)
                .addClass('btn btn-sm btn-primary btn-editar-beneficiario')
                .text('Alterar')
                .appendTo(celulaAcoes);

            celulaAcoes.append(' ');

            $('<button>')
                .attr('type', 'button')
                .attr('data-index', index)
                .addClass('btn btn-sm btn-primary btn-excluir-beneficiario')
                .text('Excluir')
                .appendTo(celulaAcoes);

            linha.append(celulaAcoes);
            tbody.append(linha);
        });
    }

    function incluirOuAlterar() {
        var cpf = $('#BenefCPF').val();
        var nome = $('#BenefNome').val();
        var cpfNormalizado = normalizarCpf(cpf);
        var duplicado = false;

        if (!validarCpf(cpf)) {
            ModalDialog('Ocorreu um erro', 'O CPF do beneficiário informado não é válido. Por favor, digite novamente');
            return;
        }

        $.each(beneficiarios, function (index, beneficiario) {
            if (index !== indiceEmEdicao &&
                normalizarCpf(beneficiario.CPF) === cpfNormalizado) {
                duplicado = true;
                return false;
            }
        });

        if (duplicado) {
            ModalDialog('Ocorreu um erro', 'Já existe um beneficiário com esse CPF.');
            return;
        }

        var beneficiarioEmEdicao = indiceEmEdicao >= 0
            ? beneficiarios[indiceEmEdicao]
            : null;

        var beneficiario = {
            Id: beneficiarioEmEdicao ? beneficiarioEmEdicao.Id : 0,
            CPF: cpf,
            Nome: nome
        };

        if (indiceEmEdicao >= 0)
            beneficiarios[indiceEmEdicao] = beneficiario;
        else
            beneficiarios.push(beneficiario);

        renderizar();
        limparCampos();
    }

    function iniciarEdicao(index) {
        var beneficiario = beneficiarios[index];

        if (!beneficiario)
            return;

        indiceEmEdicao = index;

        $('#BenefIndiceEdicao').val(index);
        $('#BenefNome').val(beneficiario.Nome || '');
        $('#BenefCPF').val(beneficiario.CPF || '').trigger('input');
        $('#btnIncluirBeneficiario').text('Alterar');
    }

    function excluir(index) {
        if (index < 0 || index >= beneficiarios.length)
            return;

        if (!window.confirm('Deseja excluir este beneficiário?'))
            return;

        var removido = beneficiarios.splice(index, 1)[0];

        if (removido && removido.Id > 0)
            idsExcluidos.push(removido.Id);

        if (indiceEmEdicao === index) {
            limparCampos();
        } else if (indiceEmEdicao > index) {
            indiceEmEdicao--;
            $('#BenefIndiceEdicao').val(indiceEmEdicao);
        }

        renderizar();
    }

    function inicializarLista(cliente) {
        var listaInicial = cliente && $.isArray(cliente.Beneficiarios)
            ? cliente.Beneficiarios
            : [];

        beneficiarios = $.map(listaInicial, function (beneficiario) {
            return {
                Id: beneficiario.Id || 0,
                CPF: beneficiario.CPF || '',
                Nome: beneficiario.Nome || ''
            };
        });
        idsExcluidos = [];

        renderizar();
        limparCampos();
    }

    function adicionarAoPayload(data) {
        $.each(beneficiarios, function (index, beneficiario) {
            data['Beneficiarios[' + index + '].Id'] = beneficiario.Id || 0;
            data['Beneficiarios[' + index + '].CPF'] = beneficiario.CPF || '';
            data['Beneficiarios[' + index + '].Nome'] = beneficiario.Nome || '';
        });

        $.each(idsExcluidos, function (index, id) {
            data['BeneficiariosExcluidos[' + index + ']'] = id;
        });

        return data;
    }

    window.BeneficiariosUI = {
        adicionarAoPayload: adicionarAoPayload,
        limpar: function () {
            beneficiarios = [];
            idsExcluidos = [];
            limparCampos();
            renderizar();
        }
    };

    $(function () {
        var cliente = window.obj || null;

        inicializarLista(cliente);

        $('#formBeneficiario').on('submit', function (e) {
            e.preventDefault();
            incluirOuAlterar();
        });

        $('#tbodyBeneficiarios').on('click', '.btn-editar-beneficiario', function () {
            iniciarEdicao(parseInt($(this).attr('data-index'), 10));
        });

        $('#tbodyBeneficiarios').on('click', '.btn-excluir-beneficiario', function () {
            excluir(parseInt($(this).attr('data-index'), 10));
        });

        $('#modalBeneficiarios').on('hidden.bs.modal', limparCampos);
    });
})(jQuery, window);