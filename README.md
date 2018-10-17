# Kuromoji.NET

Kuromoji.NET is an Japanese morphological analyzer. This is a pure C# porting of [Kuromoji](https://github.com/atilika/kuromoji).

## Warning

* This is not finished porting everything yet!
* Not compatible original Kuromoji's compiled dictionary, please use this
    * unidic: https://app.box.com/s/tgax5hsj6014g622ymynfz0esq4g8ryo
    * unidic-neologd: https://app.box.com/s/tgax5hsj6014g622ymynfz0esq4g8ryo
    * unidic-kanaaccent: https://app.box.com/s/enwcos5v7gw85angxniwuyqy35bql717

## Usage

```
using Kuromoji.NET.Tokenizers.UniDic;
using System;

namespace Kuromoji.NET.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = "お寿司が食べたい。";
            var tokenizer = new Tokenizer(@"path\to\dictionary\unidic.zip");

            Console.WriteLine($"input:\t{input}");
            foreach (var token in tokenizer.Tokenize(input))
            {
                Console.WriteLine($"{token.Surface}\t{token.GetAllFeatures()}");
            }
        }
    }
}
```

output
```
input:  お寿司が食べたい。
お      接頭辞,*,*,*,*,*,オ,御,お,オ,お,オ,和,*,*,促添,基本形
寿司    名詞,普通名詞,一般,*,*,*,スシ,寿司,寿司,スシ,寿司,スシ,和,ス濁,基本形,*,*
が      助詞,格助詞,*,*,*,*,ガ,が,が,ガ,が,ガ,和,*,*,*,*
食べ    動詞,一般,*,*,下一段-バ行,連用形-一般,タベル,食べる,食べ,タベ,食べる,タベル,和,*,*,*,*
たい    助動詞,*,*,*,助動詞-タイ,終止形-一般,タイ,たい,たい,タイ,たい,タイ,和,*,*,*,*
。      補助記号,句点,*,*,*,*,,。,。,,。,,記号,*,*,*,*
```

## License

* Apache License 2.0

## TODO

* Support ipadic, jumandic, naist-jdic
* Replace double array to [fst](https://github.com/atilika/kuromoji/tree/master/kuromoji-core/src/main/java/com/atilika/kuromoji/fst)