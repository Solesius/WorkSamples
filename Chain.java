//some kind of array thing see https://typelevel.org/cats/datatypes/chain.html
//also https://algs4.cs.princeton.edu/home/

import java.lang.StackWalker.Option;
import java.util.Arrays;
import java.util.Optional;
import java.util.Random;

public class Main {

    private static void print(Object x)
    {
        System.out.println(x);
    }
    public static void main(String[] args) throws InterruptedException {
        Chain<Object> a = new Chain<Object>();
        Chain<Object> b = new Chain<Object>();

        a.append(Integer.valueOf(1)).append("4");
        b.append(Integer.valueOf(1)).append("3");

        System.out.println(a.equals(b));

        Thread.sleep(78000);

    }

    private static class Link<T> {
        protected T data;

        public Link() {
            this.data = (T) "";
        }

        public Link(T t) {
            this.data = t;
        }

        public T data() {
            return this.data;
        };

        // public boolean equals(Object that){
        //     Link<T> _that = (Link<T>)that;
        //     return 
        // }

    }

    private static class Chain<T> {
        private Link<T>[] links = (Link<T>[]) new Link[] {};
        private int MAX_LENGTH = 128000;

        public Chain(int len) {
            this.MAX_LENGTH = (len > 0 && len < Integer.MAX_VALUE) ? len : MAX_LENGTH;
        }

        public Chain() {

        }

        public int size() {
            return this.links.length;
        }

        public T get(int idx) {
            if (idx < 0 || idx > size()){
                if (idx < 0 )
                    return get(0);
                if (idx > size())
                    return (get(size() -1));
                
                return this.links[idx].data();
            }

            Optional<T> opt = (Optional<T>) Optional.of(this.links[idx]);
            return opt.isPresent() ? ((Link<T>) opt.get()).data() : null;
        }

        public T tl() {
            return get(size() - 1);
        }

        public Chain<T> append(T data) throws UnsupportedOperationException {
            if (!(null == data) && this.links.length < MAX_LENGTH) {
                this.links = Arrays.copyOf(this.links, this.links.length + 1);
                this.links[this.links.length - 1] = new Link<T>(data);

                return this;
            } else {
                return this;
            }
        }

        public Chain<T> remove(int idx) {
            // rangecheck 0 < idx < size()
            if (idx < 0 || idx > size()){
                new IndexOutOfBoundsException("Invalid index provided: "+ Integer.toString(idx)).printStackTrace();
                return this;
            }

            Link<T>[] links0 = (Link<T>[]) new Link[size() - 1];

            // tail check
            if (idx == size()) {
                for (int i = 0; i < size() - 1; i++)
                    links0[i] = links[i];

                this.links = links0;
                links0 = null;

                return this;
            }

            // this check will also handle idx == 0 case;
            for (int i = 0; i < idx; i++) {
                links0[i] = links[i];
            }
            for (int i = idx + 1; i < size(); i++) {
                if (i == size())
                    break;
                else
                    links0[i - 1] = links[i];
            }

            this.links = links0;
            links0 = null;

            return this;
        }

        //get this method working properly, 
        //bad results. 
        public boolean equals (Object that){
            boolean valid = false;
            Chain<T> _that = ((Chain<T>)that);

            if (this.getClass() != that.getClass())
                return false;
            if (_that.size() != size())
                return false;
            
            for(int i=0; i < size(); i++){
                Link<T> a = (Link<T>)this.links[i];
                for (int j=0; j< _that.size(); j++){
                    Link<T> b = (Link<T>)_that.links[j];
                    if (a.data.getClass() == b.data.getClass() && a.data() == b.data()){   
                        valid = true;
                    }else {
                        valid = false;
                    }              
                }
            }

            return valid;
        }

        void forEachItem(java.util.function.Consumer<? super T> action) {
            for (Link<T> elem : links) {
                action.accept(elem.data());
            }
        }

        public void printEach() {
            for (int i = 0; i < this.links.length; i++) {
                System.out.println(this.links[i].data());
            }
        }

        private T getOrElseNull(Optional<T> optional) {
            return optional.isPresent() ? optional.get() : null;
        }
    }

}
