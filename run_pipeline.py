import os, sys, glob
from typing import List

from langchain_community.document_loaders import PyPDFLoader, TextLoader
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_openai import OpenAIEmbeddings, ChatOpenAI
from langchain.chains import RetrievalQA
from langchain_chroma import Chroma

DATA_DIR = os.getenv("DATA_DIR", "data")
PERSIST_DIR = os.getenv("PERSIST_DIR", "chroma_db")
MODEL_EMB = os.getenv("MODEL_EMB", "text-embedding-ada-002")  # 1536 dims
LLM_MODEL = os.getenv("LLM_MODEL", "gpt-4o-mini")
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")

def discover_files(folder: str):
    patterns = ["**/*.pdf", "**/*.txt", "**/*.md"]
    files = []
    for p in patterns:
        files.extend(glob.glob(os.path.join(folder, p), recursive=True))
    return [f for f in files if os.path.isfile(f)]

def load_documents(file_paths: List[str]):
    docs = []
    for path in file_paths:
        try:
            low = path.lower()
            if low.endswith(".pdf"):
                loader = PyPDFLoader(path)
            else:
                loader = TextLoader(path, encoding="utf-8")
            docs.extend(loader.load())
        except Exception as e:
            print(f"⚠️  Saltando {os.path.basename(path)}: {e}")
    return docs

def main():
    if not OPENAI_API_KEY:
        print("❌ Falta OPENAI_API_KEY (configúralo en GitHub Secrets).")
        return 1

    print("🔧 Iniciando pipeline…")
    print(f"📁 DATA_DIR={DATA_DIR} | PERSIST_DIR={PERSIST_DIR}")

    files = discover_files(DATA_DIR)
    if not files:
        print(f"❌ No se encontraron archivos en {DATA_DIR}. Agrega PDF/TXT/MD.")
        return 1
    print(f"📄 Archivos detectados: {len(files)}")

    docs = load_documents(files)
    if not docs:
        print("❌ No se pudieron cargar documentos.")
        return 1
    print(f"📝 Documentos cargados: {len(docs)}")

    splitter = RecursiveCharacterTextSplitter(chunk_size=1000, chunk_overlap=200)
    chunks = splitter.split_documents(docs)
    print(f"🧩 Chunks generados: {len(chunks)}")

    embeddings = OpenAIEmbeddings(model=MODEL_EMB, api_key=OPENAI_API_KEY)
    Chroma.from_documents(
        documents=chunks,
        embedding=embeddings,
        persist_directory=PERSIST_DIR
    )
    print(f"📦 Chroma creado en: {PERSIST_DIR}")

    llm = ChatOpenAI(model=LLM_MODEL, api_key=OPENAI_API_KEY, temperature=0)
    retriever = Chroma(
        persist_directory=PERSIST_DIR,
        embedding_function=embeddings
    ).as_retriever(search_kwargs={"k": 4})
    qa = RetrievalQA.from_chain_type(llm=llm, retriever=retriever)

    # mini prueba de salud
    test_queries = [
        "Dame un resumen breve del contenido cargado.",
        "Menciona 3 temas técnicos que aparecen en los documentos."
    ]
    for i, q in enumerate(test_queries, 1):
        res = qa.invoke({"query": q})
        ans = res.get("result", res)
        print(f"\n🔎 Pregunta {i}: {q}\n🧠 Respuesta:\n{ans}")

    print("\n✅ Pipeline OK.")
    return 0

if __name__ == "__main__":
    sys.exit(main())
