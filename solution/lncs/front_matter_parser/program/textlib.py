

def clean_text(input) -> str:
    def replace_double_spaces(input) -> str:
        count = input.count('  ')
        output = input.replace('  ', ' ')
        if count > 0:
            return replace_double_spaces(output)
        else:
            return output
    a = input.strip()
    b = a.replace('\n', ' ')
    c = replace_double_spaces(b)
    return c if c != '' else None