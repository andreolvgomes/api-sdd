## Arquitetura

### Projetos e responsabilidades
- Erp.Core: entidades anêmicas (só propriedades, sem métodos nem validação); interfaces de repositórios e providers. Não depende de nenhum outro projeto.
- Erp.Application.Abstractions: depende só do Core.
	- Interfaces dos services (ex.: IProdutosService.cs)
	- Casos de uso
	- DTOs de entrada e saída, na subpasta Dtos de cada área
- Erp.Application: implementação dos services e dos casos de uso. Todas as regras de negócio e validações ficam aqui.
- Erp.Infra.Data: implementação dos repositórios com Dapper e SQLite.
- Erp.Infra: integrações externas (e-mail, APIs de terceiros).
- Erp.Infra.CrossCutting: implementação dos providers (data/hora, log, criptografia...).
- Erp.Infra.CrossCutting.IoC: registro de todas as dependências.
- Erp.Api: endpoints FastEndpoints. Só recebem a requisição, chamam o service e devolvem a resposta. Nenhuma regra de negócio aqui.

### Services
- Cada área tem um service (ex.: ProdutosService) com as operações da área: incluir, alterar, excluir, obter e listar. Não criar uma classe por operação.
- Consultas simples da entidade (obter, listar) retornam a entidade direto pelo service.

### DTOs
- Ficam em Erp.Application.Abstractions/{Area}/Dtos/, nunca no mesmo arquivo de services, endpoints ou repositórios.
- Uma classe por arquivo.
- Só propriedades: sem métodos, sem validação, sem regra.
- Nomes:
	- Entrada: {Operação}{Entidade}Request (ex.: IncluirProdutoRequest, AlterarProdutoRequest, ListarProdutosRequest)
	- Saída: {Entidade}Response (ex.: ProdutoResponse)
- Os endpoints usam os Requests de Abstractions; não criam classes de request próprias.
- A conversão entre entidade e DTO fica em Erp.Application/{Area}/{Entidade}Mapeamento.cs.

Exemplo:

    // Erp.Application.Abstractions/Produtos/Dtos/IncluirProdutoRequest.cs
    namespace Erp.Application.Abstractions.Produtos.Dtos;

    public class IncluirProdutoRequest
    {
        public string Prod_codigo { get; set; } = string.Empty;
    }

    // Erp.Application.Abstractions/Produtos/Dtos/ProdutoResponse.cs
    namespace Erp.Application.Abstractions.Produtos.Dtos;

    public class ProdutoResponse
    {
        public string Prod_codigo { get; set; } = string.Empty;
    }

### Nomes de colunas, entidades e DTOs
- Colunas começam com a sigla da tabela em 4 letras + `_`, tudo minúsculo (ex.: produto → `prod_codigo`, `prod_descricao`, `prod_precovenda`; cliente → `clie_id`, `clie_nome`, `clie_telefone`).
- Chave primária tem o nome `<sigla>_id` (ex.: `clie_id`, `vend_id`). Quando já existe um campo único que identifica o registro, ele é a chave primária com o próprio nome (ex.: `prod_codigo` em produto).
- Propriedades da entidade e dos DTOs (Requests e Responses) têm o mesmo nome da coluna, só com a primeira letra maiúscula (ex.: `Prod_precovenda`, `Clie_id`).
- Assim o Dapper mapeia sem alias, inclusive em resultados de inner join.
- Siglas das tabelas: produto `prod`, cliente `clie`, endereco_adicional `ende`, vendedor `vend`, forma_pagamento `form`, caixa_banco `caix`, orcamento `orca`, orcamento_item `orci`, orcamento_pagamento `orcp`.
- Chave estrangeira usa exatamente o mesmo nome da chave primária que referencia (ex.: `clie_id` em `cliente` e em `endereco_adicional` e `orcamento`; `prod_codigo` em `produto` e em `orcamento_item`). Não existe outro nome para representar uma FK.
- Os demais campos da tabela usam a sigla da própria tabela (ex.: `ende_rotulo`, `ende_cidade` em `endereco_adicional`).

### Exemplo: onde fica cada parte de Produtos

    Erp.Core/
      Entities/Produto.cs
      Repositories/IProdutoRepository.cs
    Erp.Application.Abstractions/
      Produtos/
        IProdutosService.cs
        Dtos/
          IncluirProdutoRequest.cs
          ProdutoResponse.cs
    Erp.Application/
      Produtos/
        ProdutosService.cs
        ProdutoValidacao.cs
        ProdutoMapeamento.cs
    Erp.Infra.Data/
      Repositories/ProdutoRepository.cs
    Erp.Infra.CrossCutting.IoC/
      DependencyInjection.cs (registro de produtos)
    Erp.Api/
      Common/EndpointBase.cs
      Features/Produtos/
        IncluirProdutoEndpoint.cs
        (e os demais endpoints de produtos)

### Rotas
- /api/produtos, /api/clientes, /api/pedidos
- Os verbos decoram as classes de endpoint, sem barra inicial: [HttpPost("api/produtos")]

### Organização de endpoints por subcontexto
- Os endpoints de uma área ficam em `Erp.Api/Features/{Area}/`.
- Quando a área tem um subcontexto (ex.: Itens e Pagamentos dentro de Orçamentos), o subcontexto vai em subpasta: `Erp.Api/Features/{Area}/{Subcontexto}/`.
- O namespace acompanha a pasta: `Erp.Api.Features.{Area}.{Subcontexto}`.
- A tag continua sendo o nome da área, não do subcontexto: `[Tags("Orçamentos")]` também nos endpoints de `Itens`.
- Nome do arquivo: `{Operação}{Entidade}Endpoint.cs`.

Exemplo:

    Erp.Api/
      Features/
        Orcamentos/
          IncluirOrcamentoEndpoint.cs
          Itens/
            IncluirItemOrcamentoEndpoint.cs
          Pagamentos/
            LancarPagamentoEndpoint.cs

Um endpoint novo de um subcontexto:

    // Erp.Api/Features/Clientes/Contatos/IncluirContatoEndpoint.cs
    namespace Erp.Api.Features.Clientes.Contatos;

    [HttpPost("api/clientes/{Clie_id}/contatos")]
    [AllowAnonymous()]
    [Tags("Clientes")]
    public class IncluirContatoEndpoint : EndpointBase<IncluirContatoRequest>
    {
        // ...
    }

## Endpoints
- Todo endpoint herda de EndpointBase (Api/Common) e responde só pelos helpers: Ok, Created, BadRequest, NotFound, NoContent.
- Nunca use Send.* ou HttpContext.Response direto.

Exemplo:

    public override async Task<object?> ExecuteAsync(ObterProdutoRequest req, CancellationToken ct)
    {
        var produto = await _produtosService.ObterAsync(req);
        if (produto == null)
            return await NotFound("Produto não encontrado.", ct);

        return await Ok(produto, ct);
    }