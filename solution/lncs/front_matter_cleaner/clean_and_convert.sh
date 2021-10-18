#!/bin/bash
echo "$1"
input_dir="${1}"/input
gs_out_dir="${1}"/gs_out
mkdir -p $gs_out_dir
pdf2htmlex_out="${1}"/pdf2htmlex_out
mkdir -p $pdf2htmlex_out
echo "Input directory: $input_dir"
echo "pdf2htmlex_out directory: $pdf2htmlex_out"

for input_file_path in $input_dir/*.pdf; do
    file_name=$(basename $input_file_path)
    cln_file_name="${file_name%.*}"
    echo "Processing: $cln_file_name"

    echo "Execute GS"
    gs -sDEVICE=pdfwrite -sOutputFile="$gs_out_dir/$file_name" -dNOPAUSE -dBATCH "$input_file_path"

    echo "Execute PDF2HTMLEX"
    pdf2htmlEX --fit-width 1024 --process-nontext 0 --process-outline 0 --printing 0 --embed-external-font 0 --optimize-text 1 --dest-dir "$pdf2htmlex_out" "$gs_out_dir/$file_name" "$cln_file_name.html"

done
echo "DONE"
