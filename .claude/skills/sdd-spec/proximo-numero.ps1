# .claude/skills/sdd-spec/proximo-numero.ps1
# Retorna o próximo número de spec com três dígitos (ex.: 004).
# Deve ser executado na raiz do projeto.

if (-not (Test-Path specs)) {
    '001'
    exit
}

$numeros = Get-ChildItem specs -Directory |
    Where-Object { $_.Name -match '^\d{3}-' } |
    ForEach-Object { [int]$_.Name.Substring(0, 3) }

$maior = [int](($numeros | Measure-Object -Maximum).Maximum)
if ($maior -eq $null) {
    $maior = 0
}

'{0:D3}' -f ($maior + 1)