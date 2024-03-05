using System.Linq; // Older versions of C# might need this explicitly

namespace Fasteroid {

     /*
     * ParallelPipeline.cs - MIT License
     *
     * Copyright (c) 2024 Fasteroid
     *
     * Permission is hereby granted, free of charge, to any person obtaining a copy
     * of this software and associated documentation files (the "Software"), to deal
     * in the Software without restriction, including without limitation the rights
     * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
     * copies of the Software, and to permit persons to whom the Software is
     * furnished to do so, subject to the following conditions:
     *
     * The above copyright notice and this permission notice shall be included in all
     * copies or substantial portions of the Software.
     *
     * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
     * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
     * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
     * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
     * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
     * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
     * SOFTWARE.
    */

    public class ParallelPipeline<In, Out> {
        
        private IEnumerable<Task<Out>> Tasks { set; get; }

        private ParallelPipeline(IEnumerable<Task<Out>> Tasks) {
            this.Tasks = Tasks;
        }

        /// <summary>
        /// Runs <paramref name="process"/> on each <paramref name="input"/> member in parallel.
        /// </summary>
        /// <param name="process">Do this to each element of <paramref name="input"/></param>
        public static ParallelPipeline<In, Out> Foreach(IEnumerable<In> input, Func<In, Out> process) {
            return new ParallelPipeline<In, Out>(
                input.Select( element => Task.Run(() => process(element)) )
            );
        }

        /// <summary>
        /// Runs <paramref name="process"/> on each <paramref name="input"/> member in parallel.
        /// </summary>
        /// <param name="process">Do this to each element of <paramref name="input"/></param>
        public static ParallelPipeline<In, Out> Foreach(IEnumerable<In> input, Func<In, Task<Out>> process) {
            return new ParallelPipeline<In, Out>(
                input.Select( element => Task.Run(async () => await process(element)) )
            );
        }

        /// <summary>
        /// Waits for all elements to process, then runs <paramref name="process"/> for each result in parallel.
        /// </summary>
        /// <param name="process">Do this for each result of the previous step</param>
        public ParallelPipeline<Out, NextOut> Next<NextOut>( Func<Out, NextOut> process ) {
            return new ParallelPipeline<Out, NextOut>(
                Task.WhenAll(Tasks).ContinueWith( 
                    all => all.Result.Select( element => Task.Run(() => process(element)) ) 
                ).Result
            );
        }

        /// <summary>
        /// Waits for all elements to process, then runs <paramref name="process"/> for each result in parallel.
        /// </summary>
        /// <param name="process">Do this for each result of the previous step</param>
        public ParallelPipeline<Out, NextOut> Next<NextOut>( Func<Out, Task<NextOut>> process ) {
            return new ParallelPipeline<Out, NextOut>(
                Task.WhenAll(Tasks).ContinueWith( 
                    all => all.Result.Select( element => Task.Run(async () => await process(element)) ) 
                ).Result
            );
        }

        /// <summary>
        /// Call at the end of the pipeline and await it to eventually get the results.
        /// </summary>
        public async Task<List<Out>> Return() {
            return (await Task.WhenAll(Tasks)).ToList();
        }

    }
}
