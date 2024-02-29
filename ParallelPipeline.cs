

namespace Common {

     /*
     * ParallelPipeline by [not doxxing myself] a.k.a "Fasteroid"
     * Written on 2/29/2024
     * 
     * Allows you to chain together a series of parallel stages.
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
        public static ParallelPipeline<In, Out> StartBy(IEnumerable<In> input, Func<In, Out> process) {
            return new ParallelPipeline<In, Out>(
                input.Select( element => Task.Run(() => process(element)) )
            );
        }

        /// <summary>
        /// Waits for all elements to process, then runs <paramref name="process"/> for each result in parallel.
        /// </summary>
        /// <param name="process">Do this for each result of the previous step</param>
        public ParallelPipeline<Out, NextOut> ThenAll<NextOut>( Func<Out, NextOut> process ) {
            return new ParallelPipeline<Out, NextOut>(
                Task.WhenAll(Tasks).ContinueWith( 
                    all => all.Result.Select( element => Task.Run(() => process(element)) ) 
                ).Result
            );
        }

        /// <summary>
        /// Call at the end of the pipeline and await it to eventually get the results.
        /// </summary>
        public async Task<List<Out>> Results() {
            return (await Task.WhenAll(Tasks)).ToList();
        }

    }
}
