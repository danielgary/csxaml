import { promises as fs } from 'node:fs'
import { createRequire } from 'node:module'
import path from 'node:path'
import { fileURLToPath, pathToFileURL } from 'node:url'

const codeSelector = 'pre > code.lang-csxaml, pre > code.language-csxaml'
const highlightedSelector = 'pre[data-csxaml-highlighted="true"]'

async function main() {
  const options = parseArgs(process.argv.slice(2))
  const repoRoot = resolveRepoRoot(options.repoRoot)
  const siteRoot = options.siteRoot
    ? path.resolve(options.siteRoot)
    : path.join(repoRoot, '_site')

  await assertDirectory(siteRoot)

  const dependencies = await loadDocsDependencies(repoRoot)
  const htmlFiles = await findHtmlFiles(siteRoot)
  const summary = options.check
    ? await checkHighlightedFiles(htmlFiles, dependencies)
    : await highlightFiles(repoRoot, htmlFiles, dependencies)

  writeSummary(summary)
  failOnInvalidCheck(options, summary)
}

function parseArgs(args) {
  const options = {
    check: false,
    repoRoot: '',
    siteRoot: ''
  }

  for (let index = 0; index < args.length; index += 1) {
    const arg = args[index]
    if (arg === '--check') {
      options.check = true
      continue
    }

    if (arg === '--repo-root' || arg === '--site-root') {
      const value = args[index + 1]
      if (!value) {
        throw new Error(`Missing value for ${arg}.`)
      }

      options[toOptionName(arg)] = value
      index += 1
      continue
    }

    throw new Error(`Unknown argument '${arg}'.`)
  }

  return options
}

function toOptionName(arg) {
  return arg.substring(2).replace(/-([a-z])/g, (_, letter) => letter.toUpperCase())
}

function resolveRepoRoot(explicitRoot) {
  if (explicitRoot) {
    return path.resolve(explicitRoot)
  }

  const scriptPath = fileURLToPath(import.meta.url)
  return path.resolve(path.dirname(scriptPath), '..', '..')
}

async function assertDirectory(directory) {
  const stat = await fs.stat(directory).catch(() => undefined)
  if (!stat?.isDirectory()) {
    throw new Error(`Site root does not exist: ${directory}`)
  }
}

async function findHtmlFiles(directory) {
  const entries = await fs.readdir(directory, { withFileTypes: true })
  const files = []

  for (const entry of entries) {
    const entryPath = path.join(directory, entry.name)
    if (entry.isDirectory()) {
      files.push(...await findHtmlFiles(entryPath))
      continue
    }

    if (entry.isFile() && entry.name.endsWith('.html')) {
      files.push(entryPath)
    }
  }

  return files.sort((left, right) => left.localeCompare(right))
}

async function loadDocsDependencies(repoRoot) {
  const docsPackagePath = path.join(repoRoot, 'docs-site', 'package.json')
  const docsRequire = createRequire(docsPackagePath)
  const [cheerio, shiki] = await Promise.all([
    import(pathToFileURL(docsRequire.resolve('cheerio')).href),
    import(pathToFileURL(docsRequire.resolve('shiki')).href)
  ])

  return {
    createHighlighter: shiki.createHighlighter,
    load: cheerio.load
  }
}

async function checkHighlightedFiles(htmlFiles, dependencies) {
  const summary = createSummary(htmlFiles.length)

  for (const htmlFile of htmlFiles) {
    const html = await fs.readFile(htmlFile, 'utf8')
    const $ = dependencies.load(html, { decodeEntities: false })
    const unprocessedCount = $(codeSelector).length
    const highlightedCount = $(highlightedSelector).length

    summary.unprocessedBlocks += unprocessedCount
    summary.highlightedBlocks += highlightedCount
    addFileIssue(summary.unprocessedFiles, htmlFile, unprocessedCount)
    addFileIssue(summary.highlightedFiles, htmlFile, highlightedCount)
  }

  return summary
}

async function highlightFiles(repoRoot, htmlFiles, dependencies) {
  const highlighter = await createCsxamlHighlighter(repoRoot, dependencies)
  const summary = createSummary(htmlFiles.length)

  for (const htmlFile of htmlFiles) {
    const html = await fs.readFile(htmlFile, 'utf8')
    const $ = dependencies.load(html, { decodeEntities: false })
    let fileBlockCount = 0

    $(codeSelector).each((_, codeElement) => {
      const code = $(codeElement).text()
      const highlightedHtml = renderCsxamlBlock(highlighter, dependencies, code)
      $(codeElement).parent('pre').replaceWith(highlightedHtml)
      fileBlockCount += 1
    })

    if (fileBlockCount > 0) {
      await fs.writeFile(htmlFile, $.html(), 'utf8')
      summary.highlightedBlocks += fileBlockCount
      addFileIssue(summary.highlightedFiles, htmlFile, fileBlockCount)
    }
  }

  return summary
}

function createSummary(scannedFiles) {
  return {
    scannedFiles,
    highlightedBlocks: 0,
    highlightedFiles: [],
    unprocessedBlocks: 0,
    unprocessedFiles: []
  }
}

function addFileIssue(files, filePath, count) {
  if (count > 0) {
    files.push({ filePath, count })
  }
}

async function createCsxamlHighlighter(repoRoot, dependencies) {
  const grammarRoot = path.join(repoRoot, 'VSCodeExtension', 'syntaxes')
  const embeddedGrammar = await readGrammar(
    path.join(grammarRoot, 'csxaml-embedded-csharp.tmLanguage.json'),
    'csxaml-embedded-csharp',
    'source.csxaml.embedded.csharp',
    ['csharp']
  )
  const csxamlGrammar = await readGrammar(
    path.join(grammarRoot, 'csxaml.tmLanguage.json'),
    'csxaml',
    'source.csxaml',
    ['csharp', 'csxaml-embedded-csharp']
  )

  return dependencies.createHighlighter({
    themes: ['github-light', 'github-dark'],
    langs: ['csharp', embeddedGrammar, csxamlGrammar]
  })
}

async function readGrammar(filePath, name, scopeName, embeddedLangs) {
  const grammar = JSON.parse(await fs.readFile(filePath, 'utf8'))
  return {
    ...grammar,
    name,
    scopeName,
    embeddedLangs
  }
}

function renderCsxamlBlock(highlighter, dependencies, code) {
  const shikiHtml = highlighter.codeToHtml(code, {
    lang: 'csxaml',
    themes: {
      light: 'github-light',
      dark: 'github-dark'
    },
    defaultColor: 'light'
  })
  const $ = dependencies.load(shikiHtml, { decodeEntities: false }, false)
  const pre = $('pre').first()

  pre.addClass('csxaml-shiki')
  pre.attr('data-csxaml-highlighted', 'true')
  pre.attr('data-lang', 'csxaml')

  return $.html(pre)
}

function writeSummary(summary) {
  console.log(
    `CSXAML highlighting: scanned ${summary.scannedFiles} HTML files, ` +
    `highlighted ${summary.highlightedBlocks} blocks.`
  )

  if (summary.unprocessedBlocks > 0) {
    console.error(`Unprocessed CSXAML code blocks: ${summary.unprocessedBlocks}`)
    for (const issue of summary.unprocessedFiles) {
      console.error(`- ${issue.filePath}: ${issue.count}`)
    }
  }
}

function failOnInvalidCheck(options, summary) {
  if (!options.check) {
    return
  }

  if (summary.unprocessedBlocks > 0) {
    process.exitCode = 1
    return
  }

  if (summary.highlightedBlocks === 0) {
    console.error('No highlighted CSXAML code blocks were found.')
    process.exitCode = 1
  }
}

main().catch(error => {
  console.error(error.message)
  process.exitCode = 1
})
