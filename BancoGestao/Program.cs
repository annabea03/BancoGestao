using System;
using System.Data; 
using System.Collections.Generic; 
using MySql.Data.MySqlClient; // essencial para a comunicação com o mysql.

namespace GestaoAlunos
{
    class Program
    {
        // correção dos erros intencionais (string de conexão)
        private const string ConexaoString = "server=localhost; uid=root; pwd=root; database=escola; port=3306";

        static void Main(string[] args)
        {
            // inicia o menu principal do aplicativo.
            MenuPrincipal();
        }

        static void MenuPrincipal()
        {
            bool sair = false;
            // loop principal para manter o menu ativo.
            while (!sair)
            {
                // limpa o console antes de exibir o menu.
                Console.Clear();
                Console.WriteLine("=== gerenciador de alunos ===");
                Console.WriteLine("1. cadastrar aluno");
                Console.WriteLine("2. listar todos os alunos");
                Console.WriteLine("3. buscar aluno por nome");
                Console.WriteLine("4. atualizar aluno");
                Console.WriteLine("5. excluir aluno");
                Console.WriteLine("6. exibir total de alunos");
                Console.WriteLine("7. sair");
                Console.Write("escolha uma opção: ");

                string opcao = Console.ReadLine();

                if (opcao == "7")
                {
                    sair = true;
                    break;
                }

                try
                {
                    // 1. a conexão precisa ser declarada aqui para ser usada no bloco using
                    MySqlConnection conexao = new MySqlConnection(ConexaoString);

                    // 2. uso do bloco using tradicional para garantir que a conexão será fechada
                    using (conexao)
                    {
                        conexao.Open(); // 3. abre a conexão com o banco

                        // 4. chama o método correspondente à opção
                        switch (opcao)
                        {
                            case "1": CadastrarAluno(conexao); break;
                            case "2": ListarAlunos(conexao); break;
                            case "3": BuscarAluno(conexao); break;
                            case "4": AtualizarAluno(conexao); break;
                            case "5": ExcluirAluno(conexao); break;
                            case "6": ExibirTotalAlunos(conexao); break;
                            default: Console.WriteLine("opção inválida."); break;
                        }

                        Console.WriteLine("\npressione qualquer tecla para continuar...");
                        Console.ReadKey();
                    } // o 'using' garante que conexao.close() é chamado aqui.
                }
                catch (MySqlException ex)
                {
                    // trata erros específicos do mysql (ex: senha errada, banco offline).
                    Console.WriteLine($"\n[erro de banco de dados] falha na operação: {ex.Message}");
                    Console.WriteLine("verifique sua string de conexão, senha e se o mysql server está em execução.");
                    Console.WriteLine("\npressione qualquer tecla para continuar...");
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    // trata outros erros inesperados.
                    Console.WriteLine($"\n[erro geral] ocorreu um erro: {ex.Message}");
                    Console.WriteLine("\npressione qualquer tecla para continuar...");
                    Console.ReadKey();
                }
            }
            Console.WriteLine("programa encerrado.");
        }

        // --- métodos crud ---

        static void CadastrarAluno(MySqlConnection conexao)
        {
            Console.WriteLine("\n--- novo cadastro ---");
            // 1. leitura dos dados do console
            Console.Write("nome do aluno: ");
            string nomeDigitado = Console.ReadLine();
            Console.Write("idade do aluno: ");
            string idadeStr = Console.ReadLine();
            Console.Write("curso do aluno: ");
            string cursoDigitado = Console.ReadLine();

            // 2. validação de entrada (idade deve ser um número)
            if (!int.TryParse(idadeStr, out int idadeConvertida))
            {
                Console.WriteLine("idade inválida. cadastro cancelado.");
                return;
            }

            // 3. definição do comando insert (usando parâmetros para segurança)
            string sqlInsert = "insert into alunos (nome, idade, curso) values (@nome, @idade, @curso)";

            // 4. criação e execução do comando
            using (MySqlCommand cmd = new MySqlCommand(sqlInsert, conexao))
            {
                // 5. adição dos parâmetros
                cmd.Parameters.AddWithValue("@nome", nomeDigitado);
                cmd.Parameters.AddWithValue("@idade", idadeConvertida);
                cmd.Parameters.AddWithValue("@curso", cursoDigitado);

                // 6. executa o insert e retorna o número de linhas afetadas
                int linhasAfetadas = cmd.ExecuteNonQuery();
                Console.WriteLine($"\n✅ {linhasAfetadas} registro(s) inserido(s) com sucesso!");
            }
        }

        static void ListarAlunos(MySqlConnection conexao)
        {
            // 1. definição da query select
            string sqlSelect = "select id, nome, idade, curso from alunos order by nome";

            // 2. criação do comando
            using (MySqlCommand cmd = new MySqlCommand(sqlSelect, conexao))
            {
                // 3. execução da query e reader
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("\nnenhum aluno cadastrado.");
                        return;
                    }

                    Console.WriteLine("\n--- lista de alunos ---");
                    // 4. iteração sobre os resultados (lê linha por linha)
                    while (reader.Read())
                    {
                        // 5. extração dos dados por nome da coluna
                        int id = reader.GetInt32("id");
                        string nome = reader.GetString("nome");
                        int idade = reader.GetInt32("idade");
                        string curso = reader.GetString("curso");

                        Console.WriteLine($"id: {id} | nome: {nome} | idade: {idade} | curso: {curso}");
                    }
                    Console.WriteLine("-----------------------");
                }
            }
        }

        static void BuscarAluno(MySqlConnection conexao)
        {
            Console.WriteLine("\n--- buscar aluno ---");
            Console.Write("digite o nome ou parte do nome para buscar: ");
            string termoBusca = Console.ReadLine();

            // 1. definição da query com like e parâmetro
            string sqlSelect = "select id, nome, idade, curso from alunos where nome like @termoBusca order by nome";

            using (MySqlCommand cmd = new MySqlCommand(sqlSelect, conexao))
            {
                // 2. o '%{0}%' permite buscar a string em qualquer parte do campo (ex: 'maria' encontra 'anna maria').
                cmd.Parameters.AddWithValue("@termoBusca", $"%{termoBusca}%");

                // 3. execução e leitura
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine($"\nnenhum aluno encontrado para o termo '{termoBusca}'.");
                        return;
                    }

                    Console.WriteLine("\n--- resultados da busca ---");
                    while (reader.Read())
                    {
                        Console.WriteLine($"id: {reader.GetInt32("id")} | nome: {reader.GetString("nome")} | idade: {reader.GetInt32("idade")} | curso: {reader.GetString("curso")}");
                    }
                    Console.WriteLine("---------------------------");
                }
            }
        }

        static void AtualizarAluno(MySqlConnection conexao)
        {
            Console.WriteLine("\n--- atualizar aluno ---");
            // 1. solicita o id para atualização
            Console.Write("digite o id do aluno que deseja atualizar: ");
            if (!int.TryParse(Console.ReadLine(), out int idParaAtualizar))
            {
                Console.WriteLine("id inválido.");
                return;
            }

            // 2. coleta os novos dados (permite deixar vazio para manter o valor)
            Console.Write("novo nome (ou enter para manter): ");
            string novoNome = Console.ReadLine();
            Console.Write("nova idade (ou enter para manter): ");
            string novaIdadeStr = Console.ReadLine();
            Console.Write("novo curso (ou enter para manter): ");
            string novoCurso = Console.ReadLine();

            // verifica se há algo para atualizar
            if (string.IsNullOrWhiteSpace(novoNome) && string.IsNullOrWhiteSpace(novaIdadeStr) && string.IsNullOrWhiteSpace(novoCurso))
            {
                Console.WriteLine("nenhuma informação para atualizar. operação cancelada.");
                return;
            }

            // 3. busca os dados atuais do aluno 
            string sqlCheck = "select nome, idade, curso from alunos where id = @id";
            string nomeAtual = "";
            int idadeAtual = 0;
            string cursoAtual = "";
            bool alunoEncontrado = false;

            using (MySqlCommand cmdCheck = new MySqlCommand(sqlCheck, conexao))
            {
                cmdCheck.Parameters.AddWithValue("@id", idParaAtualizar);

                using (MySqlDataReader reader = cmdCheck.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        alunoEncontrado = true;
                        nomeAtual = reader.GetString("nome");
                        idadeAtual = reader.GetInt32("idade");
                        cursoAtual = reader.GetString("curso");
                    }
                } // reader.close() é chamado aqui.
            }

            if (!alunoEncontrado)
            {
                Console.WriteLine($"aluno com id {idParaAtualizar} não encontrado.");
                return;
            }

            // 4. lógica para definir os valores finais (se novo valor vazio, usa o atual)
            string nomeFinal = string.IsNullOrWhiteSpace(novoNome) ? nomeAtual : novoNome;
            int idadeFinal = idadeAtual;
            if (!string.IsNullOrWhiteSpace(novaIdadeStr) && int.TryParse(novaIdadeStr, out int idadeConvertida))
            {
                idadeFinal = idadeConvertida;
            }

            string cursoFinal = string.IsNullOrWhiteSpace(novoCurso) ? cursoAtual : novoCurso;

            // 5. executa o update
            string sqlUpdate = "update alunos set nome = @nome, idade = @idade, curso = @curso where id = @id";
            using (MySqlCommand cmdUpdate = new MySqlCommand(sqlUpdate, conexao))
            {
                // 6. adiciona todos os parâmetros para o comando update
                cmdUpdate.Parameters.AddWithValue("@id", idParaAtualizar);
                cmdUpdate.Parameters.AddWithValue("@nome", nomeFinal);
                cmdUpdate.Parameters.AddWithValue("@idade", idadeFinal);
                cmdUpdate.Parameters.AddWithValue("@curso", cursoFinal);

                int linhasAfetadas = cmdUpdate.ExecuteNonQuery();

                if (linhasAfetadas > 0)
                {
                    Console.WriteLine($"\n✅ aluno id {idParaAtualizar} atualizado com sucesso!");
                }
                else
                {
                    Console.WriteLine($"\nnenhuma alteração aplicada.");
                }
            }
        }

        static void ExcluirAluno(MySqlConnection conexao)
        {
            Console.WriteLine("\n--- excluir aluno ---");
            // 1. solicita o id para exclusão
            Console.Write("digite o id do aluno que deseja excluir: ");
            if (!int.TryParse(Console.ReadLine(), out int idParaExcluir))
            {
                Console.WriteLine("id inválido.");
                return;
            }

            // 2. confirmação de exclusão
            Console.Write($"tem certeza que deseja excluir o aluno id {idParaExcluir}? (s/n): ");
            if (Console.ReadLine().ToUpper() != "S")
            {
                Console.WriteLine("exclusão cancelada.");
                return;
            }

            // 3. definição do comando delete
            string sqlDelete = "delete from alunos where id = @id";
            using (MySqlCommand cmd = new MySqlCommand(sqlDelete, conexao))
            {
                // 4. adiciona o parâmetro id
                cmd.Parameters.AddWithValue("@id", idParaExcluir);

                // 5. executa o delete
                int linhasAfetadas = cmd.ExecuteNonQuery();

                if (linhasAfetadas > 0)
                {
                    Console.WriteLine($"\n✅ aluno id {idParaExcluir} excluído com sucesso!");
                }
                else
                {
                    Console.WriteLine($"\n❌ aluno com id {idParaExcluir} não encontrado.");
                }
            }
        }

        static void ExibirTotalAlunos(MySqlConnection conexao)
        {
            // 1. definição da query count
            string sqlCount = "select count(*) from alunos";
            // 2. criação do comando
            using (MySqlCommand cmd = new MySqlCommand(sqlCount, conexao))
            {
                // 3. executescalar retorna a primeira coluna da primeira linha (a contagem)
                object resposta = cmd.ExecuteScalar();
                // 4. converte a resposta para inteiro
                int total = Convert.ToInt32(resposta);

                Console.WriteLine($"\ntotal de alunos cadastrados: {total}");
            }
        }
    }
}