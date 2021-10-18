gs -sDEVICE=pdfwrite -sOutputFile='gs_out/1.pdf' -dNOPAUSE -dBATCH 01_input_pdf/1.pdf

pdf2htmlEX --fit-width 1024 --process-nontext 0 --process-outline 0 --printing 0 --embed-external-font 0 --optimize-text 1  gs_out/2021_Bookmatter_ApplicationsOfEvolutionaryComp.pdf pdf2htmlex_out/2021_Bookmatter_ApplicationsOfEvolutionaryComp.html

activate virtual environment:
source ./venv/bin/activate

install requirements:
python -m pip install -r requirements.txt
