require 'prism'
require 'find'
require 'set'

# Classe especializada herdando de Prism::Visitor
class ExtratorVisitor < Prism::Visitor
  def initialize(caminho_arquivo, variaveis_por_escopo, inclusoes_por_classe, arquivos_por_variavel, classes_detectadas, globais_detectadas)
    @caminho = caminho_arquivo
    @variaveis_por_escopo = variaveis_por_escopo
    @inclusoes_por_classe = inclusoes_por_classe
    @arquivos_por_variavel = arquivos_por_variavel
    @classes_detectadas = classes_detectadas
    @globais_detectadas = globais_detectadas
    @escopo_atual = []
    super()
  end

  # Quando encontra uma Classe
  def visit_class_node(node)
    nome_classe = node.constant_path.slice
    @escopo_atual << nome_classe
    @classes_detectadas << @escopo_atual.join('::')
    super(node)
    @escopo_atual.pop
  end

  # Quando encontra um Módulo
  def visit_module_node(node)
    nome_modulo = node.constant_path.slice
    @escopo_atual << nome_modulo
    super(node)
    @escopo_atual.pop
  end

  # Quando encontra uma chamada de método (include ou attr_*)
  def visit_call_node(node)
    # 1. Tratamento para Includes
    if node.name == :include && @escopo_atual.any?
      classe_pai = @escopo_atual.join('::')
      node.arguments&.arguments&.each do |arg|
        @inclusoes_por_classe[classe_pai] << arg.slice if arg.respond_to?(:slice)
      end
    end

    # 2. Tratamento para attr_accessor, attr_reader e attr_writer
    if [:attr_accessor, :attr_reader, :attr_writer].include?(node.name) && @escopo_atual.any?
      node.arguments&.arguments&.each do |arg|
        next unless arg.respond_to?(:slice)
        
        nome_limpo = arg.slice.delete_prefix(':').delete_prefix('"').delete_suffix('"')
        # Marcamos temporariamente com o asterisco para o relatório
        nome_var_attr = "@*#{nome_limpo}"
        
        registrar_variavel(nome_var_attr, por_attr: true)
      end
    end

    super(node)
  end

  # Captura escrita de variável de instância: @variavel = valor
  def visit_instance_variable_write_node(node)
    registrar_variavel(node.name.to_s, por_attr: false)
    super(node)
  end

  # Captura leitura de variável de instância: return @variavel
  def visit_instance_variable_read_node(node)
    registrar_variavel(node.name.to_s, por_attr: false)
    super(node)
  end

  # Captura atribuição condicional: @variavel ||= valor
  def visit_instance_variable_operator_write_node(node)
    registrar_variavel(node.name.to_s, por_attr: false)
    super(node)
  end

  # Captura escrita de Variável Global
  def visit_global_variable_write_node(node)
    registrar_global(node.name)
    super(node)
  end

  # Captura leitura de Variável Global
  def visit_global_variable_read_node(node)
    registrar_global(node.name)
    super(node)
  end

  # Captura atribuição condicional de Variável Global
  def visit_global_variable_operator_write_node(node)
    registrar_global(node.name)
    super(node)
  end

  private

  def registrar_variavel(nome_var, por_attr:)
    return unless @escopo_atual.any?
    escopo = @escopo_atual.join('::')
    
    if por_attr
      # Se ela veio por attr_*, só adicionamos se a versão literal (@nome) ainda não existir
      versao_literal = nome_var.sub('*', '')
      unless @variaveis_por_escopo[escopo].include?(versao_literal)
        @variaveis_por_escopo[escopo] << nome_var
        @arquivos_por_variavel[[escopo, nome_var]] << @caminho
      end
    else
      # Se ela veio de forma literal (@nome), ela tem prioridade total.
      versao_attr = nome_var.sub('@', '@*')
      
      # Se a versão com asterisco já tinha sido guardada antes por um attr_*, nós a removemos e limpamos o cache
      if @variaveis_por_escopo[escopo].include?(versao_attr)
        @variaveis_por_escopo[escopo].delete(versao_attr)
        arquivos_antigos = @arquivos_por_variavel.delete([escopo, versao_attr]) || Set.new
        @arquivos_por_variavel[[escopo, nome_var]].merge(arquivos_antigos)
      end

      @variaveis_por_escopo[escopo] << nome_var
      @arquivos_por_variavel[[escopo, nome_var]] << @caminho
    end
  end

  def registrar_global(nome_global)
    @globais_detectadas[nome_global] << @caminho
  end
end

class MapeadorProjeto
  def initialize(diretorio_raiz = '.')
    @diretorio_raiz = diretorio_raiz
    @variaveis_por_escopo = Hash.new { |h, k| h[k] = Set.new }
    @inclusoes_por_classe = Hash.new { |h, k| h[k] = Set.new }
    @arquivos_por_variavel = Hash.new { |h, k| h[k] = Set.new }
    @classes_detectadas = Set.new
    @globais_detectadas = Hash.new { |h, k| h[k] = Set.new }
  end

  def executar!
    puts "1. Escaneando pastas recursivamente e alimentando cache estático..."
    escanear_arquivos

    puts "2. Resolvendo heranças e mixins (Módulos -> Classes)..."
    resolver_modulos_inclusos

    puts "3. Exportando relatório textual..."
    gerar_relatorio_txt
  end

  private

  def escanear_arquivos
    Find.find(@diretorio_raiz) do |caminho|
      if File.directory?(caminho)
        if ['node_modules', '.git', 'vendor', 'tmp', 'log', 'coverage'].include?(File.basename(caminho))
          Find.prune
        else
          next
        end
      end

      next unless caminho.end_with?('.rb')
      next if caminho.include?('extractor.rb') || caminho.include?('mapeador_prism.rb')

      processar_arquivo(caminho)
    end
  end

  def processar_arquivo(caminho)
    resultado = Prism.parse_file(caminho)
    return unless resultado.success?

    visitor = ExtratorVisitor.new(
      caminho, 
      @variaveis_por_escopo, 
      @inclusoes_por_classe, 
      @arquivos_por_variavel, 
      @classes_detectadas,
      @globais_detectadas
    )
    
    resultado.value.accept(visitor)
  rescue StandardError => e
    puts "Erro ao processar #{caminho}: #{e.message}"
  end

  def resolver_modulos_inclusos
    @inclusoes_por_classe.each do |classe, modulos|
      modulos.each do |modulo|
        modulo_real = encontrar_modulo_no_cache(modulo)
        next unless modulo_real

        @variaveis_por_escopo[modulo_real].each do |var|
          # Ao herdar do módulo, se a classe já usa de forma literal (@nome), não sobrescrevemos com a do módulo que está com asterisco
          versao_literal = var.sub('*', '')
          versao_attr = var.include?('*') ? var : var.sub('@', '@*')

          if var.include?('*') # É uma variável de attr_* do módulo
            next if @variaveis_por_escopo[classe].include?(versao_literal)
            @variaveis_por_escopo[classe] << var
          else # É uma variável literal do módulo
            if @variaveis_por_escopo[classe].include?(versao_attr)
              @variaveis_por_escopo[classe].delete(versao_attr)
              arquivos_antigos = @arquivos_por_variavel.delete([classe, versao_attr]) || Set.new
              @arquivos_por_variavel[[classe, var]].merge(arquivos_antigos)
            end
            @variaveis_por_escopo[classe] << var
          end

          arquivos_do_modulo = @arquivos_por_variavel[[modulo_real, var]]
          @arquivos_por_variavel[[classe, var]].merge(arquivos_do_modulo)
        end
      end
    end
  end

  def encontrar_modulo_no_cache(nome_modulo)
    return nome_modulo if @variaveis_por_escopo.key?(nome_modulo)
    @variaveis_por_escopo.keys.find { |k| k.end_with?("::#{nome_modulo}") }
  end

  def gerar_relatorio_txt
    nome_arquivo = "relatorio_variaveis_projeto.txt"
    
    File.open(nome_arquivo, "w") do |f|
      f.puts "====================================================================="
      f.puts "   RELATÓRIO DE VARIÁVEIS DE INSTÂNCIA E VARIÁVEIS GLOBAIS"
      f.puts "   Nota: Variaveis com '@*' foram geradas via attr_* sem uso literal de @"
      f.puts "   Gerado em: #{Time.now.strftime('%d/%m/%Y %H:%M:%S')}"
      f.puts "=====================================================================\n\n"

      f.puts "=== VARIÁVEIS GLOBAIS DETECTADAS ==="
      if @globais_detectadas.empty?
        f.puts "  (Nenhuma variável global detectada)"
      else
        @globais_detectadas.sort.each do |global, arquivos|
          f.puts "  - #{global.to_s.ljust(25)} [Arquivos: #{arquivos.to_a.join(', ')}]"
        end
      end
      f.puts "=" * 80
      f.puts "\n"

      f.puts "=== MAPEAMENTO DE CLASSES E MÓDULOS ==="
      if @classes_detectadas.empty?
        f.puts "Nenhuma classe mapeada no projeto."
      else
        @classes_detectadas.sort.each do |classe|
          f.puts "Classe: #{classe}"
          
          if @inclusoes_por_classe[classe].any?
            f.puts "  Módulos Inclusos: #{@inclusoes_por_classe[classe].to_a.join(', ')}"
          end
          
          variaveis = @variaveis_por_escopo[classe]
          
          if variaveis.empty?
            f.puts "  (Nenhuma variável de instância detectada)"
          else
            f.puts "  Variáveis e suas Origens:"
            variaveis.sort.each do |var|
              arquivos = @arquivos_por_variavel[[classe, var]].to_a.join(', ')
              f.puts "    - #{var.ljust(25)} [Arquivos: #{arquivos}]"
            end
          end
          f.puts "-" * 80
        end
      end
    end

    puts "Sucesso! Relatório atualizado gerado em: #{nome_arquivo}"
  end
end

MapeadorProjeto.new('.').executar!