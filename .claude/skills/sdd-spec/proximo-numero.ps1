# .claude/skills/sdd-spec/proximo-numero.ps1
# Retorna o próximo número de spec com três dígitos (ex.: 004).
# Conta as pastas de specs/ e de specs/em-andamento/. Deve ser executado na raiz do projeto.

$pastas = @()

if (Test-Path specs) {
    $pastas += Get-ChildItem specs -Directory
}

if (Test-Path specs/em-andamento) {
    $pastas += Get-ChildItem specs/em-andamento -Directory
}

if ($pastas.Count -eq 0) {
    '001'
    exit
}

$numeros = $pastas |
    Where-Object { $_.Name -notlike '_*' } |
    Where-Object { $_.Name -match '^\d{3}-' } |
    ForEach-Object { [int]$_.Name.Substring(0, 3) }

$maior = [int](($numeros | Measure-Object -Maximum).Maximum)

'{0:D3}' -f ($maior + 1)
