using BERTTokenizers;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;


namespace MyApp // Note: actual namespace depends on the project name.
{
    public class Library
    {
        static Semaphore semaphore = new Semaphore(1, 1);
        // Get path to model to create inference session.
        private static string modelPath = "C:\\Users\\vanap\\Downloads\\bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        private static string modelLink = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        private static InferenceSession session;
        CancellationToken token;
        public Library(CancellationToken token_)
        {
            token = token_;
        }

        public async Task Init_model()
        {
            if (!File.Exists(modelPath))
            {
                await Download_Model();
            }
            session = new InferenceSession(modelPath);
        }
        public Task<String> Dialog(string text, string question)
        {
            return Task.Factory.StartNew(() =>
            {
                token.ThrowIfCancellationRequested();
                var sentence = "{\"question\": \"" + question + "\", \"context\": \"@CTX\"}".Replace("@CTX", text);
                Console.WriteLine(sentence);
                // Создаем токенизатор и токенизируем предложение.
                var tokenizer = new BertUncasedLargeTokenizer();
                // Получаем токены предложений.
                var tokens = tokenizer.Tokenize(sentence);
                // Console.WriteLine(String.Join(", ", tokens));
                // Кодируем предложение и передаем количество токенов в предложении.
                var encoded = tokenizer.Encode(tokens.Count(), sentence);
                // Выделение кодировки на InputIds, AttentionMask и TypeIds из списка (input_id, alert_mask, type_id).
                var bertInput = new BertInput()
                {
                    InputIds = encoded.Select(t => t.InputIds).ToArray(),
                    AttentionMask = encoded.Select(t => t.AttentionMask).ToArray(),
                    TypeIds = encoded.Select(t => t.TokenTypeIds).ToArray(),
                };


                // Создаем входной тензор.

                var input_ids = ConvertToTensor(bertInput.InputIds, bertInput.InputIds.Length);
                var attention_mask = ConvertToTensor(bertInput.AttentionMask, bertInput.InputIds.Length);
                var token_type_ids = ConvertToTensor(bertInput.TypeIds, bertInput.InputIds.Length);


                // Создаем входные данные для сеанса.
                var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", input_ids),
                                                    NamedOnnxValue.CreateFromTensor("input_mask", attention_mask),
                                                    NamedOnnxValue.CreateFromTensor("segment_ids", token_type_ids) };



                // Запускаем сеанс и отправляем входные данные, чтобы получить вывод вывода. 
                token.ThrowIfCancellationRequested();
                IDisposableReadOnlyCollection<DisposableNamedOnnxValue> output;
                semaphore.WaitOne();
                try
                {
                    output = session.Run(input);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in predict model");
                    semaphore.Release();
                    throw;
                }
                Console.WriteLine("Model predict");
                semaphore.Release();
                token.ThrowIfCancellationRequested();
                // Вызов ToList на выходе.
                // Получаем первый и последний элемент в списке.
                // Получаем значение элемента и приводим его к IEnumerable<float>, чтобы получить результат списка.
                List<float> startLogits = (output.ToList().First().Value as IEnumerable<float>).ToList();
                List<float> endLogits = (output.ToList().Last().Value as IEnumerable<float>).ToList();

                // Получаем индекс максимального значения из выходных списков.
                var startIndex = startLogits.ToList().IndexOf(startLogits.Max());
                var endIndex = endLogits.ToList().IndexOf(endLogits.Max());

                // Из списка исходных токенов в предложении
                // Получаем токены между startIndex и endIndex и преобразуем в словарь идентификатор токена.
                var predictedTokens = tokens
                            .Skip(startIndex)
                            .Take(endIndex + 1 - startIndex)
                            .Select(o => tokenizer.IdToToken((int)o.VocabularyIndex))
                            .ToList();

                // Распечатываем результат.
                var res = String.Join(" ", predictedTokens);
                token.ThrowIfCancellationRequested();
                return res;
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        static Tensor<long> ConvertToTensor(long[] inputArray, int inputDimension)
        {
            // Создайте тензор той формы, которую ожидает модель. Здесь мы отправляем 1 пакет с inputDimension в качестве количества токенов.
            Tensor<long> input = new DenseTensor<long>(new[] { 1, inputDimension });

            // Проходим по входному массиву (InputIds, AttentionMask и TypeIds)
            for (var i = 0; i < inputArray.Length; i++)
            {
                // Добавляем каждый из входных результатов Тенора.
                // Устанавливаем индекс и значение массива для каждого входного тензора.
                input[0, i] = inputArray[i];
            }
            return input;
        }
        private static async Task Download_Model()
        {
            int iteration = 0;
            while (iteration < 5)
            {
                try
                {
                    var client = new HttpClient();
                    var getstream = await client.GetStreamAsync(modelLink);
                    var filestream = new FileStream(modelPath, FileMode.CreateNew);
                    await getstream.CopyToAsync(filestream);
                    return;
                }
                catch(Exception e)
                { 
                    iteration++;
                    break;
                }
            }
        }
    }

    public class BertInput
    {
        public long[] InputIds { get; set; }
        public long[] AttentionMask { get; set; }
        public long[] TypeIds { get; set; }
    }
}